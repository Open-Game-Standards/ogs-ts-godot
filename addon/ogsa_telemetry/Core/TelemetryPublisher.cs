#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Numerics;

namespace Ogsa.Telemetry
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
/// Engine-agnostic OGSA-TS publisher: two-phase wiring over a
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
    bool _locked;

    // discovery environment (schema: "may be re-sent on change") — defaults
    // match the original hardcoded discovery values
    float _envAirDensity = 1.225f, _envTemperature = 20f, _envPressure = 1.01325f, _envGravity = 9.81f;
    Schema.ContactMedium _envMedium = Schema.ContactMedium.Asphalt;

    Func<Vector3>? _getPosition;
    Func<(Vector3 specificForce, Vector3 angularVelocityRadS)>? _getBodyDynamics;
    Func<Quaternion>? _getOrientation;

    readonly Queue<(Schema.Event ev, double at)> _recentEvents = new();
    readonly List<Schema.Event> _eventScratch = new(); // reused re-carrier list
    uint _nextEventId = 1;

    // Cached wire frames. The object graphs are structurally identical on
    // every send (declarations lock at the first tick), so they are built
    // once and refilled in place — the 60-240 Hz motion path stops
    // allocating a frame graph per tick.
    Schema.DiscoveryFrame? _discoveryFrame;
    Schema.MotionFrame? _motionFrame;
    List<Schema.ChannelValue>? _motionValues;

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
            _discoveryFrame.Objects![0].TypeLabel = typeLabel;
        _lastDiscovery = double.NegativeInfinity; // re-send at the next tick
    }

    /// <summary>Declares or updates the ambient environment carried in
    /// discovery. Callable before the first tick and afterwards — the
    /// schema allows the environment to be re-sent on change ("e.g.
    /// entering water"), so a changed environment re-sends discovery at
    /// the next tick, like <see cref="SetObjectTypeLabel"/>. Senders with
    /// per-level conditions should call it on level load: a space level
    /// would set gravity 0 and <c>ContactMedium.vacuum</c>. All values are
    /// taken as given — no defaults are guessed on a partial update.</summary>
    public void SetEnvironment(float gravity, float airDensity, float temperature,
                               float pressure, Schema.ContactMedium medium)
    {
        if (_envGravity == gravity && _envAirDensity == airDensity &&
            _envTemperature == temperature && _envPressure == pressure &&
            _envMedium == medium) return;
        _envGravity = gravity; _envAirDensity = airDensity;
        _envTemperature = temperature; _envPressure = pressure;
        _envMedium = medium;
        if (_discoveryFrame is not null)
        {
            var env = _discoveryFrame.Environment;
            env.Gravity = gravity; env.AirDensity = airDensity;
            env.Temperature = temperature; env.Pressure = pressure;
            env.Medium = medium;
        }
        _lastDiscovery = double.NegativeInfinity; // re-send at the next tick
    }

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

        if (nowSeconds - _lastDiscovery >= _discoveryInterval)
        {
            _sender.Send(FrameType.Discovery, BuildDiscovery());
            _lastDiscovery = nowSeconds;
        }

        if (nowSeconds - _lastMotion >= _motionInterval)
        {
            _sender.Send(FrameType.Motion, BuildMotion(nowSeconds));
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
        _getOrientation = null;
        _discoveryFrame = null;
        _motionFrame = null;
        _motionValues = null;
    }

    // Built once: every field is fixed per publisher (declarations are
    // locked before the first send) and the encoder only reads. The
    // descriptor lists are referenced live rather than copied — they are
    // immutable after the lock.
    Schema.DiscoveryFrame BuildDiscovery() => _discoveryFrame ??= new Schema.DiscoveryFrame
    {
        SchemaVersion = 1,
        GameName = _gameName,
        SessionStartUnixUs = _sessionStartTicks / 10, // .NET ticks (100 ns) → µs
        Environment =
        {
            AirDensity = _envAirDensity, Temperature = _envTemperature,
            Pressure = _envPressure, Gravity = _envGravity, Medium = _envMedium,
        },
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

    Schema.MotionFrame BuildMotion(double nowSeconds)
    {
        if (_motionFrame is null || _motionValues!.Count != _bindings.Count)
        {
            // (re)build the cached graph: one object, one value per binding;
            // setting a value re-selects its union variant, so the cached
            // ChannelValues encode exactly like fresh ones
            _motionValues = new List<Schema.ChannelValue>(_bindings.Count);
            for (int i = 0; i < _bindings.Count; i++)
                _motionValues.Add(new Schema.ChannelValue { Id = (ushort)i });
            _motionFrame = new Schema.MotionFrame
            {
                Objects = new List<Schema.MotionObject>
                {
                    new() { Name = _objectName, Channels = _motionValues },
                },
            };
        }

        var obj = _motionFrame.Objects![0];
        // an unbound pose encodes as zeros — what receivers read for an
        // absent pose anyway
        var p = _getPosition?.Invoke() ?? default;
        obj.Position.X = p.X; obj.Position.Y = p.Y; obj.Position.Z = p.Z;
        var q = _getOrientation?.Invoke() ?? default;
        obj.Orientation.X = q.X; obj.Orientation.Y = q.Y;
        obj.Orientation.Z = q.Z; obj.Orientation.W = q.W;
        if (_getBodyDynamics is not null)
        {
            var (sf, av) = _getBodyDynamics();
            var bd = obj.InitBodyDynamics();
            bd.SpecificForce.X = sf.X; bd.SpecificForce.Y = sf.Y; bd.SpecificForce.Z = sf.Z;
            bd.AngularVelocity.X = av.X; bd.AngularVelocity.Y = av.Y; bd.AngularVelocity.Z = av.Z;
        }
        else obj.SetNoDynamics();

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

        _motionFrame.Timestamp = nowSeconds - _anchor;
        return _motionFrame;
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
