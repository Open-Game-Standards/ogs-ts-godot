#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Numerics;

namespace Ogs.Telemetry
{

/// <summary>Handle to a declared channel; pass to Bind/Set methods.</summary>
public sealed class ChannelHandle
{
    internal ChannelHandle(int id, string name, Unit unit, bool boolean)
    {
        Id = id; Name = name; Unit = unit; Boolean = boolean;
    }
    public int Id { get; }
    public string Name { get; }
    public Unit Unit { get; }
    public bool Boolean { get; }
}

/// <summary>
/// Engine-agnostic OGS-TS publisher: two-phase wiring over a
/// <see cref="TelemetrySender"/>.
///
/// 1. Declare (setup): the object, optional wheel drive points, and channels
///    (name + required target unit) — becomes the low-rate DiscoveryFrame.
/// 2. Feed (per frame): bound getters are sampled on the publisher's tick
///    (pull model) and/or values are pushed via SetChannel; converted values
///    ride the high-rate MotionFrame / EventFrame.
///
/// Bind-time source units are converted via <see cref="Conversions"/> with
/// factors precomputed at bind; sample-time cost is one multiply per channel.
/// The host drives cadence by calling <see cref="Tick(double)"/> with a
/// monotonic clock (engine wrappers do this from their update loop);
/// declarations lock after the first tick because receivers cache channel
/// descriptors by id.
///
/// Every datagram stays within the 1,400-byte budget
/// (implementation-udp.md § Datagram budget): a descriptor set too large
/// for one datagram is paged (all pages at the first tick and on a change,
/// then spread across each re-send interval), and a tick with more channel
/// values than one datagram holds goes out as back-to-back parts with the
/// pose in part 0. <see cref="Generation"/> increments whenever the object's
/// identity changes.
/// </summary>
public sealed class TelemetryPublisher : IDisposable
{
    readonly TelemetrySender _sender;
    readonly string _gameName;
    readonly double _motionInterval;
    readonly double _discoveryInterval;
    readonly long _sessionStartTicks = DateTimeOffset.UtcNow.UtcTicks;
    readonly Stopwatch _clock = new();

    sealed class Binding
    {
        public int Id;
        public Func<double>? Get;              // pull model (source units)
        public Func<double, double>? Convert;  // precomputed source→target
        public double Pushed;                  // push model (target units)
        public bool Boolean;
        public bool BoolValue;
        public bool BoolPushed;
    }
    readonly List<Binding> _bindings = new();
    readonly List<Schema.ChannelDescriptor> _descriptors = new();
    readonly List<Schema.DrivePointDescriptor> _drivePoints = new();
    string _objectName = "player";
    string _typeLabel = "";
    string _instanceLabel = "";
    string _host = "";
    bool _locked;
    ChannelHandle? _pauseChannel;

    // discovery environment (schema: "may be re-sent on change") — no
    // atmosphere until SetEnvironment declares one (game engines rarely
    // model air), and standard gravity, the value BodyDynamicsDeriver
    // assumes by default
    float _envAirDensity, _envTemperature, _envPressure,
          _envGravity = BodyDynamicsDeriver.Gravity;
    bool _envAtmosphere;
    Vector3? _envGravityVector; // null = along world −Y (noGravityVector)
    Schema.ContactMedium _envMedium = Schema.ContactMedium.Asphalt;

    Func<Vector3>? _getLocalGravity;

    Func<Vector3>? _getPosition;
    Func<(Vector3 specificForce, Vector3 angularVelocityRadS)>? _getBodyDynamics;
    Func<Quaternion>? _getOrientation;

    readonly Queue<(Schema.Event ev, double at)> _recentEvents = new();
    readonly List<Schema.Event> _eventScratch = new(); // reused re-carrier list
    uint _nextEventId = 1;

    // Cached wire frames. The object graphs are structurally identical on
    // every send (declarations lock at the first tick), so they are built
    // once and refilled in place — the 60-240 Hz motion path stops
    // allocating a frame graph per tick. Discovery is cached as its pages
    // (one unpaged frame for a set that fits a datagram); motion as its
    // parts, which share the ChannelValue instances of _motionValues.
    Schema.DiscoveryFrame? _discoveryFrame;      // the complete set (unpaged view)
    List<Schema.DiscoveryFrame>? _discoveryPages;
    List<Schema.MotionFrame>? _motionParts;
    List<Schema.ChannelValue>? _motionValues;
    uint _generation;
    bool _sendAllPages = true;                   // first tick, then on any change
    int _nextPage;                               // next page of the current re-send cycle

    double _anchor;                 // host clock value at first tick
    double _lastDiscovery = double.NegativeInfinity;
    double _lastMotion = double.NegativeInfinity;
    double _lastEventSend = double.NegativeInfinity;
    double _lastTick;
    bool _ticked;

    public TelemetryPublisher(TelemetrySender sender, string gameName,
                              double motionHz = 60.0,
                              double discoveryIntervalSeconds = 2.0)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        if (motionHz <= 0) throw new ArgumentOutOfRangeException(nameof(motionHz));
        _gameName = gameName;
        _motionInterval = 1.0 / motionHz;
        _discoveryInterval = discoveryIntervalSeconds;
    }

    // ---- declaration phase (before first Tick) ----

    /// <summary>Identifies this sender instance in receivers' session
    /// pickers and pinned choices (implementation-udp.md § Senders and
    /// sessions): a user-set label such as "Rig 2" when more than one
    /// instance can run on the network, and the machine's host name
    /// (null = this machine's, "" = none). Both may stay empty.</summary>
    public void SetInstance(string instanceLabel, string? host = null)
    {
        LockCheck("SetInstance");
        _instanceLabel = instanceLabel ?? "";
        _host = host ?? LocalHostName();
    }

    /// <summary>This machine's host name, or "" where the platform has none.</summary>
    public static string LocalHostName()
    {
        try { return System.Net.Dns.GetHostName() ?? ""; }
        catch (Exception) { return ""; }
    }

    public void DeclareObject(string name, string typeLabel = "")
    {
        LockCheck("DeclareObject");
        _objectName = name; _typeLabel = typeLabel;
    }

    /// <summary>Updates the object's display label after declarations
    /// have locked — for senders that discover what they are tracking at
    /// runtime (injected mods surfacing the pawn's class name). The object
    /// NAME stays stable (receivers match discovery/motion by it); the label
    /// rides the next discovery re-send per the standard's
    /// re-send-on-change rule.</summary>
    public void SetObjectTypeLabel(string typeLabel)
    {
        if (_typeLabel == typeLabel) return;
        _typeLabel = typeLabel;
        if (_discoveryFrame is not null)
        {
            // object identity changed: a new generation, every page re-sent
            _discoveryFrame.Objects![0].TypeLabel = typeLabel;
            _generation++;
            _discoveryPages = null;
            if (_motionParts is not null)
                foreach (var part in _motionParts) part.Generation = _generation;
        }
        _sendAllPages = true;
    }

    /// <summary>The descriptor-set generation discovery and motion frames
    /// carry; increments whenever the object's identity changes.</summary>
    public uint Generation => _generation;

    /// <summary>Pages the current descriptor set occupies (1 = unpaged).
    /// Before the first tick it is computed from the declarations so far.</summary>
    public int DiscoveryPageCount => _locked ? BuildDiscoveryPages().Count : PaginateNow().Count;

    /// <summary>Declares or updates the ambient environment carried in
    /// discovery. Callable before the first tick and afterwards — the
    /// schema allows the environment to be re-sent on change ("e.g.
    /// entering water"), so a changed environment re-sends discovery at
    /// the next tick, like <see cref="SetObjectTypeLabel"/>. Senders with
    /// per-level conditions should call it on level load: a space level
    /// would set gravity 0, air density 0 and <c>ContactMedium.vacuum</c>.
    /// All values are taken as given — no defaults are guessed on a partial
    /// update. The air values declare a known atmosphere; without a call
    /// discovery declares none (<c>noAtmosphere</c>).
    /// <paramref name="gravity"/> is the magnitude, acting along world −Y;
    /// use the vector overload for any other direction.</summary>
    public void SetEnvironment(float gravity, float airDensity, float temperature,
                               float pressure, Schema.ContactMedium medium)
        => ApplyEnvironment(gravity, null, true, airDensity, temperature, pressure, medium);

    /// <summary>As the scalar overload, with gravity as a world-frame
    /// vector (m/s², pointing the way gravity pulls; CONVENTIONS C-12).
    /// Discovery carries it as <c>gravityVector</c> and its magnitude as
    /// <c>gravity</c>, so receivers that read only the scalar still get the
    /// right value. For gravity that varies by position or faster than
    /// discovery re-sends, use <see cref="BindLocalGravity"/>.</summary>
    public void SetEnvironment(Vector3 gravity, float airDensity, float temperature,
                               float pressure, Schema.ContactMedium medium)
        => ApplyEnvironment(gravity.Length(), gravity, true, airDensity, temperature, pressure, medium);

    /// <summary>Updates only the session gravity (world frame, m/s²,
    /// pointing the way gravity pulls), keeping the other environment
    /// values — for engines whose physics exposes a gravity vector (Unity
    /// Physics.gravity). Gravity straight down along −Y is declared as the
    /// scalar alone; any other direction also as <c>gravityVector</c>.
    /// Re-sends discovery when it changes.</summary>
    public void SetGravity(Vector3 gravity)
    {
        bool down = gravity.X == 0f && gravity.Z == 0f && gravity.Y <= 0f;
        ApplyEnvironment(gravity.Length(), down ? null : gravity, _envAtmosphere,
                         _envAirDensity, _envTemperature, _envPressure, _envMedium);
    }

    void ApplyEnvironment(float gravity, Vector3? gravityVector, bool atmosphere, float airDensity,
                          float temperature, float pressure, Schema.ContactMedium medium)
    {
        if (_envGravity == gravity && _envGravityVector == gravityVector &&
            _envAtmosphere == atmosphere &&
            _envAirDensity == airDensity && _envTemperature == temperature &&
            _envPressure == pressure && _envMedium == medium) return;
        _envGravity = gravity; _envGravityVector = gravityVector; _envAtmosphere = atmosphere;
        _envAirDensity = airDensity; _envTemperature = temperature;
        _envPressure = pressure; _envMedium = medium;
        if (_discoveryFrame is not null) FillEnvironment(_discoveryFrame.Environment);
        if (_discoveryPages is not null)
            foreach (var page in _discoveryPages) FillEnvironment(page.Environment);
        _sendAllPages = true; // every page repeats the environment
    }

    void FillEnvironment(Schema.Environment env)
    {
        env.Gravity = _envGravity;
        env.Medium = _envMedium;
        var atmosphere = env.Atmosphere;
        if (_envAtmosphere)
        {
            var air = atmosphere.InitKnown();
            air.AirDensity = _envAirDensity; air.Temperature = _envTemperature; air.Pressure = _envPressure;
        }
        else atmosphere.SetNoAtmosphere();
        if (_envGravityVector is { } g)
        {
            var v = env.InitGravityVector();
            v.X = g.X; v.Y = g.Y; v.Z = g.Z;
        }
        else env.SetNoGravityVector();
    }

    /// <summary>The session gravity declared in discovery, as a world-frame
    /// vector (CONVENTIONS C-12).</summary>
    public Vector3 EnvironmentGravity =>
        _envGravityVector ?? new Vector3(0f, -_envGravity, 0f);

    /// <summary>The object's effective gravity (CONVENTIONS C-12): the
    /// bound local gravity (its getter, read now), else the environment's.
    /// Feed it to <see cref="BodyDynamicsDeriver.GravityVector"/> so
    /// derived specific force matches what the stream declares.</summary>
    public Vector3 EffectiveGravity =>
        _getLocalGravity is not null ? _getLocalGravity() : EnvironmentGravity;

    public ChannelHandle DeclareChannel(string name, Unit unit,
                                        string description = "",
                                        double min = double.NaN, double max = double.NaN)
    {
        LockCheck("DeclareChannel");
        if (!unit.IsTargetUnit())
            throw new ArgumentException(
                $"{unit} is a source-only unit; declare the channel in its target " +
                $"unit and pass {unit} to BindChannel as the source", nameof(unit));
        return AddDescriptor(name, unit, description, min, max, boolean: false);
    }

    public ChannelHandle DeclareBooleanChannel(string name, string description = "")
    {
        LockCheck("DeclareBooleanChannel");
        return AddDescriptor(name, Unit.Scalar, description,
                             double.NaN, double.NaN, boolean: true);
    }

    /// <summary>The ratified pause channel (CHANNELS.md, CONVENTIONS C-11).</summary>
    public const string PausedChannelName = "session.paused";

    /// <summary>Declares <c>session.paused</c>, for hosts that can tell
    /// when their game is paused (CONVENTIONS C-11). Idempotent; like any
    /// declaration it must happen before the first tick. Engine wrappers
    /// call it for you.</summary>
    public void DeclarePause()
    {
        if (_pauseChannel is not null) return;
        LockCheck("DeclarePause");
        _pauseChannel = DeclareBooleanChannel(PausedChannelName,
            "Simulation paused for the player (C-11)");
    }

    /// <summary>Whether the game is currently reported as paused.</summary>
    public bool Paused { get; private set; }

    /// <summary>Reports the game's pause state (CONVENTIONS C-11). Keep
    /// ticking while paused: motion frames keep flowing with the frozen
    /// pose, the pause channel set and no dynamics, so receivers bring
    /// actuators to rest instead of treating the silence as a dead
    /// session. Declares the channel on first use; after the first tick
    /// that requires an earlier <see cref="DeclarePause"/>.</summary>
    public void SetPaused(bool paused)
    {
        if (_pauseChannel is null)
        {
            if (_locked)
                throw new InvalidOperationException(
                    "SetPaused after the first Tick without DeclarePause — " +
                    "receivers cache descriptors by id, so declare the pause " +
                    "channel before the first tick");
            DeclarePause();
        }
        Paused = paused;
        SetChannel(_pauseChannel!, paused);
    }

    public void AddWheel(string name, Vector3 offset, float radius,
                         bool driven, bool steered,
                         float stiffness = 65000f, float damping = 5000f)
    {
        LockCheck("AddWheel");
        var dp = new Schema.DrivePointDescriptor
        {
            Id = (ushort)_drivePoints.Count,
            Name = name,
            Offset = { X = offset.X, Y = offset.Y, Z = offset.Z },
        };
        var wheel = dp.InitWheel();
        wheel.Radius = radius; wheel.Driven = driven; wheel.Steered = steered;
        var suspension = wheel.InitSuspension();
        suspension.Travel.Min = -0.05; suspension.Travel.Max = 0.15;
        suspension.Stiffness = stiffness; suspension.Damping = damping;
        _drivePoints.Add(dp);
    }

    ChannelHandle AddDescriptor(string name, Unit unit, string description,
                                double min, double max, bool boolean)
    {
        var b = new Binding { Id = _bindings.Count, Boolean = boolean };
        _bindings.Add(b);
        var descriptor = new Schema.ChannelDescriptor
        {
            Id = (ushort)b.Id,
            Name = name,
            Unit = unit.ToWireString(),
            Description = description,
            Type = boolean ? Schema.ChannelType.Boolean : Schema.ChannelType.Number,
        };
        if (!double.IsNaN(min) && !double.IsNaN(max))
        {
            var range = descriptor.InitRange();
            range.Min = min; range.Max = max;
        }
        _descriptors.Add(descriptor);
        return new ChannelHandle(b.Id, name, unit, boolean);
    }

    void LockCheck(string what)
    {
        if (_locked)
            throw new InvalidOperationException(
                $"{what} after the first Tick — declarations are locked once " +
                "discovery has been sent (receivers cache descriptors by id)");
    }

    // ---- binding phase (pull model) ----

    /// <summary>Binds the object pose getters in the standard world frame:
    /// left-handed, Z-forward, Y-up, metres (engines convert before this).</summary>
    public void BindPose(Func<Vector3>? position, Func<Quaternion>? orientation)
    {
        _getPosition = position; _getOrientation = orientation;
    }

    /// <summary>Binds body-frame dynamics: specific force (m/s²,
    /// accelerometer-style, gravity reaction included) and angular
    /// velocity (rad/s, C-4 axes: x +nose-up pitch, y +nose-right yaw,
    /// z +roll right-down). Senders that natively measure these should
    /// bind them: receivers consume native angular rates without
    /// euler-artifacts, and motion-cueing tilt coordination feeds on
    /// specific force directly.</summary>
    public void BindBodyDynamics(
        Func<(Vector3 specificForce, Vector3 angularVelocityRadS)> get)
        => _getBodyDynamics = get;

    /// <summary>Binds the object's local gravity (world frame, m/s²,
    /// pointing the way gravity pulls; CONVENTIONS C-12) for gravity that
    /// varies by position or changes faster than discovery re-sends —
    /// planet surfaces, rotating habitats, multi-body scenes. While bound,
    /// every motion frame carries it as <c>localGravity.vector</c>; a zero
    /// vector is weightless. Pass null to fall back to the environment
    /// gravity. Bound specific force must be computed against the same
    /// gravity (see <see cref="EffectiveGravity"/>).</summary>
    public void BindLocalGravity(Func<Vector3>? get) => _getLocalGravity = get;

    /// <summary>Binds a numeric channel. The getter returns values in
    /// <paramref name="sourceUnit"/> (what the game natively measures); the
    /// factor to the channel's target unit is precomputed here and applied
    /// on every sample.</summary>
    public void BindChannel(ChannelHandle channel, Func<double> source,
                            Unit sourceUnit)
    {
        var b = GetBinding(channel, expectBoolean: false);
        var convert = Conversions.TryGetConverter(sourceUnit, channel.Unit)
            ?? throw new ArgumentException(
                $"no conversion {sourceUnit} → {channel.Unit} for channel " +
                $"{channel.Name}; convert inside the getter or bind the target unit",
                nameof(sourceUnit));
        b.Get = source; b.Convert = convert;
    }

    /// <summary>Binds a numeric channel whose getter already yields the
    /// channel's target unit (zero-overhead path).</summary>
    public void BindChannelRaw(ChannelHandle channel, Func<double> source)
    {
        var b = GetBinding(channel, expectBoolean: false);
        b.Get = source; b.Convert = null;
    }

    public void BindBooleanChannel(ChannelHandle channel, Func<bool> source)
    {
        var b = GetBinding(channel, expectBoolean: true);
        b.Get = () => source() ? 1.0 : 0.0;
        b.Convert = null;
        b.Boolean = true;
    }

    // ---- push model ----

    /// <summary>Pushes a value already in the channel's target unit (exact
    /// timing control from the game's own tick).</summary>
    public void SetChannel(ChannelHandle channel, double value)
    {
        GetBinding(channel, expectBoolean: false).Pushed = value;
    }

    public void SetChannel(ChannelHandle channel, bool value)
    {
        var b = GetBinding(channel, expectBoolean: true);
        b.BoolValue = value; b.BoolPushed = true;
    }

    /// <summary>Queues a discrete stimulus; re-carried ~100 ms per
    /// CONVENTIONS C-10 (replay + id dedup over the lossy transport).
    /// <paramref name="direction"/> points toward the stimulus source in the
    /// object's body frame; null = omnidirectional (<c>noDirection</c>).</summary>
    public void FireEvent(string name, float intensity = 1f, float duration = 0f,
                          Vector3? direction = null)
    {
        var ev = new Schema.Event
        {
            Id = _nextEventId++,
            Name = name,
            Intensity = intensity,
            Duration = duration,
        };
        if (direction is { } d)
        {
            var v = ev.InitDirection();
            v.X = d.X; v.Y = d.Y; v.Z = d.Z;
        }
        _recentEvents.Enqueue((ev, _lastTick));
    }

    // ---- cadence ----

    /// <summary>Advances the publisher with the host's monotonic clock
    /// (seconds). Sends discovery at the declared interval and motion at the
    /// configured rate, independent of how often the host ticks.</summary>
    public void Tick(double nowSeconds)
    {
        if (!_ticked) { _anchor = nowSeconds; _ticked = true; }
        _lastTick = nowSeconds;
        _locked = true;

        SendDiscovery(nowSeconds);

        if (nowSeconds - _lastMotion >= _motionInterval)
        {
            foreach (var part in BuildMotion(nowSeconds))
                _sender.Send(FrameType.Motion, part);
            _lastMotion = nowSeconds;
        }

        while (_recentEvents.Count > 0 && nowSeconds - _recentEvents.Peek().at > 0.100)
            _recentEvents.Dequeue();
        if (_recentEvents.Count > 0 && nowSeconds - _lastEventSend >= 0.050)
        {
            _eventScratch.Clear();
            foreach (var (ev, _) in _recentEvents)
                _eventScratch.Add(ev);
            _sender.Send(FrameType.Event, new Schema.EventFrame
            {
                Timestamp = nowSeconds - _anchor,
                Events = _eventScratch,
            });
            _lastEventSend = nowSeconds;
        }
    }

    /// <summary>Tick with the internal stopwatch (demos, non-engine hosts).
    /// Mixing the two overloads is not supported — pick one clock.</summary>
    public void Tick()
    {
        if (!_clock.IsRunning) _clock.Start();
        Tick(_clock.Elapsed.TotalSeconds);
    }

    /// <summary>Clears bindings and cached frames. Does not dispose the
    /// sender — the host owns it.</summary>
    public void Dispose()
    {
        _bindings.Clear();
        _recentEvents.Clear();
        _getPosition = null;
        _getBodyDynamics = null;
        _getLocalGravity = null;
        _getOrientation = null;
        _discoveryFrame = null;
        _discoveryPages = null;
        _motionParts = null;
        _motionValues = null;
    }

    // Pacing (implementation-udp.md § Send cadence): all pages back-to-back
    // at the first tick and after any change; otherwise each re-send cycle
    // spreads the pages evenly across the discovery interval.
    void SendDiscovery(double nowSeconds)
    {
        var pages = BuildDiscoveryPages();
        if (_sendAllPages)
        {
            foreach (var page in pages) _sender.Send(FrameType.Discovery, page);
            _sendAllPages = false;
            _lastDiscovery = nowSeconds;
            _nextPage = pages.Count;
            return;
        }
        if (nowSeconds - _lastDiscovery >= _discoveryInterval)
        {
            _lastDiscovery = nowSeconds;
            _nextPage = 0;
        }
        double spacing = _discoveryInterval / pages.Count;
        while (_nextPage < pages.Count && nowSeconds - _lastDiscovery >= _nextPage * spacing)
            _sender.Send(FrameType.Discovery, pages[_nextPage++]);
    }

    // Built once: every field is fixed per publisher (declarations are
    // locked before the first send) and the encoder only reads. The
    // descriptor lists are referenced live rather than copied — they are
    // immutable after the lock.
    Schema.DiscoveryFrame BuildDiscovery()
    {
        if (_discoveryFrame is null)
        {
            _discoveryFrame = NewDiscovery();
            FillEnvironment(_discoveryFrame.Environment);
        }
        return _discoveryFrame;
    }

    List<Schema.DiscoveryFrame> BuildDiscoveryPages() =>
        _discoveryPages ??= PaginateNow();

    List<Schema.DiscoveryFrame> PaginateNow()
    {
        var complete = _locked ? BuildDiscovery() : NewDiscovery();
        return DiscoveryPager.Paginate(() =>
        {
            var page = NewDiscovery();
            FillEnvironment(page.Environment);
            return page;
        }, complete.Objects!, _generation);
    }

    Schema.DiscoveryFrame NewDiscovery() => new Schema.DiscoveryFrame
    {
        SchemaVersion = 1,
        GameName = _gameName,
        SessionStartUnixUs = _sessionStartTicks / 10, // .NET ticks (100 ns) → µs
        InstanceLabel = _instanceLabel,
        Host = _host,
        Objects = new List<Schema.ObjectDescriptor>
        {
            new()
            {
                Name = _objectName,
                Type = Schema.ObjectType.Vehicle,
                TypeLabel = _typeLabel,
                Primary = true,
                Location = "local",
                ReferencePoint = { Name = "origin" },
                DrivePoints = _drivePoints,
                Channels = _descriptors,
            },
        },
    };

    List<Schema.MotionFrame> BuildMotion(double nowSeconds)
    {
        if (_motionParts is null || _motionValues!.Count != _bindings.Count)
            BuildMotionParts();

        var obj = _motionParts![0].Objects![0];
        // an unbound pose encodes as zeros — what receivers read for an
        // absent pose anyway
        var p = _getPosition?.Invoke() ?? default;
        obj.Position.X = p.X; obj.Position.Y = p.Y; obj.Position.Z = p.Z;
        var q = _getOrientation?.Invoke() ?? default;
        obj.Orientation.X = q.X; obj.Orientation.Y = q.Y;
        obj.Orientation.Z = q.Z; obj.Orientation.W = q.W;
        // C-11: nothing is measured while the simulation is stopped, and a
        // derivative across the pause boundary is meaningless
        if (_getBodyDynamics is not null && !Paused)
        {
            var (sf, av) = _getBodyDynamics();
            var bd = obj.InitBodyDynamics();
            bd.SpecificForce.X = sf.X; bd.SpecificForce.Y = sf.Y; bd.SpecificForce.Z = sf.Z;
            bd.AngularVelocity.X = av.X; bd.AngularVelocity.Y = av.Y; bd.AngularVelocity.Z = av.Z;
        }
        else obj.SetNoDynamics();

        // C-12: local gravity describes its own frame only, so it rides
        // every motion frame while bound
        if (_getLocalGravity is not null)
        {
            var local = _getLocalGravity();
            var g = obj.LocalGravity.InitVector();
            g.X = local.X; g.Y = local.Y; g.Z = local.Z;
        }
        else obj.LocalGravity.SetNoLocalGravity();

        // v1 streams all declared channels each tick; the schema permits
        // sending only changed values once senders have hundreds of channels.
        var values = _motionValues!;
        for (int i = 0; i < _bindings.Count; i++)
        {
            var b = _bindings[i];
            if (b.Boolean)
                values[i].Boolean = b.Get is not null
                    ? b.Get() != 0.0
                    : b.BoolValue;
            else
                values[i].Number = (float)(b.Get is not null
                    ? (b.Convert is not null ? b.Convert(b.Get()) : b.Get())
                    : b.Pushed);
        }

        foreach (var part in _motionParts) part.Timestamp = nowSeconds - _anchor;
        return _motionParts;
    }

    // (Re)builds the cached part graphs: one value per binding, split into
    // parts by MotionPartPlanner. Setting a value re-selects its union
    // variant, so the cached ChannelValues encode exactly like fresh ones.
    void BuildMotionParts()
    {
        _motionValues = new List<Schema.ChannelValue>(_bindings.Count);
        for (int i = 0; i < _bindings.Count; i++)
            _motionValues.Add(new Schema.ChannelValue { Id = (ushort)i });

        // worst case for part 0: body dynamics and a local gravity vector selected
        var probe0 = new Schema.MotionObject { Name = _objectName,
            Channels = new List<Schema.ChannelValue> { new() { Number = 1f } } };
        probe0.InitBodyDynamics().SpecificForce.X = 1f;
        probe0.LocalGravity.InitVector().X = 1f;
        var probeLater = new Schema.MotionObject { Name = _objectName,
            Channels = new List<Schema.ChannelValue> { new() { Number = 1f } } };
        var plan = MotionPartPlanner.Plan(_motionValues.Count,
            new Schema.MotionFrame { Timestamp = 1, Objects = new List<Schema.MotionObject> { probe0 } },
            new Schema.MotionFrame { Timestamp = 1, Objects = new List<Schema.MotionObject> { probeLater } });

        bool parts = plan.Count > 1;
        _motionParts = new List<Schema.MotionFrame>(plan.Count);
        for (int p = 0; p < plan.Count; p++)
        {
            var (start, count) = plan[p];
            _motionParts.Add(new Schema.MotionFrame
            {
                Generation = _generation,
                Part = parts ? (ushort)p : (ushort)0,
                PartCount = parts ? (ushort)plan.Count : (ushort)0,
                Objects = new List<Schema.MotionObject>
                {
                    new() { Name = _objectName, Channels = _motionValues.GetRange(start, count) },
                },
            });
        }
    }

    Binding GetBinding(ChannelHandle channel, bool expectBoolean)
    {
        if (channel.Id < 0 || channel.Id >= _bindings.Count)
            throw new ArgumentException($"unknown channel handle: {channel.Name}",
                nameof(channel));
        var b = _bindings[channel.Id];
        if (expectBoolean && !b.Boolean)
            throw new ArgumentException($"{channel.Name} is a numeric channel",
                nameof(channel));
        if (!expectBoolean && b.Boolean)
            throw new ArgumentException($"{channel.Name} is a boolean channel",
                nameof(channel));
        return b;
    }
}
}
