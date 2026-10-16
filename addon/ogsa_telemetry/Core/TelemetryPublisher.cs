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
    readonly List<CapnpGen.ChannelDescriptor> _descriptors = new();
    readonly List<CapnpGen.DrivePointDescriptor> _drivePoints = new();
    string _objectName = "player";
    string _typeLabel = "";
    bool _locked;

    Func<Vector3>? _getPosition;
    Func<(Vector3 specificForce, Vector3 angularVelocityRadS)>? _getBodyDynamics;
    Func<Quaternion>? _getOrientation;

    readonly Queue<(CapnpGen.Event ev, double at)> _recentEvents = new();
    readonly List<CapnpGen.Event> _eventScratch = new(); // reused re-carrier list
    uint _nextEventId = 1;

    // Cached wire frames. The object graphs are structurally identical on
    // every send (declarations lock at the first tick), so they are built
    // once and refilled in place — the 60-240 Hz motion path stops
    // allocating a frame graph per tick.
    CapnpGen.DiscoveryFrame? _discoveryFrame;
    CapnpGen.MotionFrame? _motionFrame;
    List<CapnpGen.ChannelValue>? _motionValues;

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
            _discoveryFrame.Objects[0].TypeLabel = typeLabel;
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
        _drivePoints.Add(new CapnpGen.DrivePointDescriptor
        {
            Id = (ushort)_drivePoints.Count,
            Name = name,
            Offset = new CapnpGen.Vector3 { X = offset.X, Y = offset.Y, Z = offset.Z },
            Wheel = new CapnpGen.WheelSpec
            {
                Radius = radius, Driven = driven, Steered = steered,
                Suspension = new CapnpGen.SuspensionSpec
                {
                    Travel = new CapnpGen.Range { Min = -0.05, Max = 0.15 },
                    Stiffness = stiffness, Damping = damping,
                },
            },
        });
    }

    ChannelHandle AddDescriptor(string name, Unit unit, string description,
                                double min, double max, bool boolean)
    {
        var b = new Binding { Id = _bindings.Count, Boolean = boolean };
        _bindings.Add(b);
        _descriptors.Add(new CapnpGen.ChannelDescriptor
        {
            Id = (ushort)b.Id,
            Name = name,
            Unit = unit.ToWireString(),
            Description = description,
            // generated code is not nullable-annotated: null = no range
            Range = double.IsNaN(min) || double.IsNaN(max)
                ? null!
                : new CapnpGen.Range { Min = min, Max = max },
        });
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
    /// CONVENTIONS C-10 (replay + id dedup over the lossy transport).</summary>
    public void FireEvent(string name, float intensity = 1f, float duration = 0f,
                          Vector3 direction = default)
    {
        _recentEvents.Enqueue((new CapnpGen.Event
        {
            Id = _nextEventId++,
            Name = name,
            Intensity = intensity,
            Duration = duration,
            Direction = new CapnpGen.Vector3
                { X = direction.X, Y = direction.Y, Z = direction.Z },
            Object = "",
        }, _lastTick));
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
            var mb = Capnp.MessageBuilder.Create(512);
            BuildDiscovery().serialize(mb.BuildRoot<CapnpGen.DiscoveryFrame.WRITER>());
            _sender.Send(FrameType.Discovery, mb);
            _lastDiscovery = nowSeconds;
        }

        if (nowSeconds - _lastMotion >= _motionInterval)
        {
            var mb = Capnp.MessageBuilder.Create(512);
            BuildMotion(nowSeconds).serialize(mb.BuildRoot<CapnpGen.MotionFrame.WRITER>());
            _sender.Send(FrameType.Motion, mb);
            _lastMotion = nowSeconds;
        }

        while (_recentEvents.Count > 0 && nowSeconds - _recentEvents.Peek().at > 0.100)
            _recentEvents.Dequeue();
        if (_recentEvents.Count > 0 && nowSeconds - _lastEventSend >= 0.050)
        {
            var mb = Capnp.MessageBuilder.Create(512);
            _eventScratch.Clear();
            foreach (var (ev, _) in _recentEvents)
                _eventScratch.Add(ev);
            new CapnpGen.EventFrame
            {
                Timestamp = nowSeconds - _anchor,
                Events = _eventScratch,
            }.serialize(mb.BuildRoot<CapnpGen.EventFrame.WRITER>());
            _sender.Send(FrameType.Event, mb);
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
    // locked before the first send) and the serializer only reads. The
    // descriptor lists are referenced live rather than copied — they are
    // immutable after the lock.
    CapnpGen.DiscoveryFrame BuildDiscovery() => _discoveryFrame ??= new CapnpGen.DiscoveryFrame
    {
        SchemaVersion = 1,
        GameName = _gameName,
        SessionStartUnixUs = _sessionStartTicks / 10, // .NET ticks (100 ns) → µs
        Environment = new CapnpGen.Environment
        {
            AirDensity = 1.225f, Temperature = 20f, Pressure = 1.01325f,
            Gravity = 9.81f, Medium = CapnpGen.ContactMedium.asphalt,
        },
        Objects = new List<CapnpGen.ObjectDescriptor>
        {
            new()
            {
                Name = _objectName,
                Type = CapnpGen.ObjectType.vehicle,
                TypeLabel = _typeLabel,
                Primary = true,
                Location = "local",
                ReferencePoint = new CapnpGen.NamedPoint
                    { Name = "origin", Position = new CapnpGen.Vector3() },
                DrivePoints = _drivePoints,
                Channels = _descriptors,
            },
        },
    };

    CapnpGen.MotionFrame BuildMotion(double nowSeconds)
    {
        if (_motionFrame is null || _motionValues!.Count != _bindings.Count)
        {
            // (re)build the cached graph: one object, one value per binding,
            // union discriminants are rewritten on every assignment so the
            // cached ChannelValues serialize exactly like fresh ones
            _motionValues = new List<CapnpGen.ChannelValue>(_bindings.Count);
            for (int i = 0; i < _bindings.Count; i++)
                _motionValues.Add(new CapnpGen.ChannelValue { Id = (ushort)i });
            var o = new CapnpGen.MotionObject { Name = _objectName };
            if (_motionValues.Count > 0) o.Channels = _motionValues;
            _motionFrame = new CapnpGen.MotionFrame
            {
                Objects = new List<CapnpGen.MotionObject> { o },
            };
        }

        var obj = _motionFrame.Objects[0];
        if (_getPosition is not null)
        {
            var p = _getPosition();
            if (obj.Position is null) obj.Position = new CapnpGen.Vector3();
            obj.Position.X = p.X; obj.Position.Y = p.Y; obj.Position.Z = p.Z;
        }
        else obj.Position = null!;
        if (_getOrientation is not null)
        {
            var q = _getOrientation();
            if (obj.Orientation is null) obj.Orientation = new CapnpGen.Quaternion();
            obj.Orientation.X = q.X; obj.Orientation.Y = q.Y;
            obj.Orientation.Z = q.Z; obj.Orientation.W = q.W;
        }
        else obj.Orientation = null!;
        if (_getBodyDynamics is not null)
        {
            var (sf, av) = _getBodyDynamics();
            if (obj.BodyDynamics is null) obj.BodyDynamics = new CapnpGen.BodyDynamics();
            obj.BodyDynamics.SpecificForce ??= new CapnpGen.Vector3();
            obj.BodyDynamics.AngularVelocity ??= new CapnpGen.Vector3();
            obj.BodyDynamics.SpecificForce.X = sf.X;
            obj.BodyDynamics.SpecificForce.Y = sf.Y;
            obj.BodyDynamics.SpecificForce.Z = sf.Z;
            obj.BodyDynamics.AngularVelocity.X = av.X;
            obj.BodyDynamics.AngularVelocity.Y = av.Y;
            obj.BodyDynamics.AngularVelocity.Z = av.Z;
            obj.which = CapnpGen.MotionObject.WHICH.BodyDynamics;
        }
        else if (obj.which == CapnpGen.MotionObject.WHICH.BodyDynamics)
            obj.which = CapnpGen.MotionObject.WHICH.NoDynamics;

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
