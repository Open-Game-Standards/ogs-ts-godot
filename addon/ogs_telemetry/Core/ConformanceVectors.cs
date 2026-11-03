#nullable enable
using System.Collections.Generic;
using Ogs.Telemetry.Schema;
using Ogs.Telemetry.Wire;

namespace Ogs.Telemetry
{

/// <summary>One conformance-corpus frame: the standard repo's
/// <c>conformance/vectors/&lt;Id&gt;.hex</c> holds its datagram.</summary>
public sealed class ConformanceVector
{
    public ConformanceVector(string id, FrameType type, WireStruct frame)
    {
        Id = id; Type = type; Frame = frame;
    }
    public string Id { get; }
    public FrameType Type { get; }
    public WireStruct Frame { get; }

    /// <summary>The datagram this runtime encodes for the vector — must
    /// equal the corpus file byte for byte.</summary>
    public byte[] Encode() => CanonicalEncoder.EncodeDatagram(Type, Frame);
}

/// <summary>
/// The conformance corpus's frames, built with the schema writers.
/// ConformanceGen writes the corpus from these; engine hosts (Unity, Godot,
/// BepInEx) encode them on their own runtime and byte-compare against the
/// committed corpus to prove that runtime's encoder agrees. The manifest
/// inputs that describe them live in ConformanceGen. All floats are
/// float32-exact.
/// </summary>
public static class ConformanceVectors
{
    public const long SessionStartUnixUs = 1_730_000_000_000_000; // 2024-10-27

    public static IReadOnlyList<ConformanceVector> All() => new[]
    {
        new ConformanceVector("discovery-minimal", FrameType.Discovery, DiscoveryMinimal()),
        new ConformanceVector("discovery-channels", FrameType.Discovery, DiscoveryChannels()),
        new ConformanceVector("motion-pose", FrameType.Motion, MotionPose()),
        new ConformanceVector("motion-channels", FrameType.Motion, MotionChannels()),
        new ConformanceVector("motion-multi", FrameType.Motion, MotionMulti()),
        new ConformanceVector("motion-bodydynamics", FrameType.Motion, MotionBodyDynamics()),
        new ConformanceVector("event-impact", FrameType.Event, EventImpact()),
        new ConformanceVector("discovery-gravity-vector", FrameType.Discovery, DiscoveryGravityVector()),
        new ConformanceVector("motion-local-gravity", FrameType.Motion, MotionLocalGravity()),
        new ConformanceVector("motion-local-gravity-zero", FrameType.Motion, MotionLocalGravityZero()),
        new ConformanceVector("discovery-paged-p0", FrameType.Discovery, DiscoveryPaged(0)),
        new ConformanceVector("discovery-paged-p1", FrameType.Discovery, DiscoveryPaged(1)),
        new ConformanceVector("motion-parts-p0", FrameType.Motion, MotionPart(0)),
        new ConformanceVector("motion-parts-p1", FrameType.Motion, MotionPart(1)),
        new ConformanceVector("discovery-identity", FrameType.Discovery, DiscoveryIdentity()),
        new ConformanceVector("motion-wheel", FrameType.Motion, MotionWheel()),
        new ConformanceVector("motion-kinematics", FrameType.Motion, MotionKinematics()),
        new ConformanceVector("discovery-drivepoints", FrameType.Discovery, DiscoveryDrivePoints()),
        new ConformanceVector("motion-drivepoints", FrameType.Motion, MotionDrivePoints()),
        new ConformanceVector("motion-texts", FrameType.Motion, MotionTexts()),
    };

    /// <summary>Generation of the paged discovery and motion-parts vectors.</summary>
    public const uint PagedGeneration = 3;

    static DiscoveryFrame Discovery(ObjectDescriptor obj)
    {
        var d = new DiscoveryFrame
        {
            SchemaVersion = 1,
            GameName = "conformance",
            SessionStartUnixUs = SessionStartUnixUs,
            Environment = { Gravity = 9.5f, Medium = ContactMedium.Asphalt },
            Objects = new List<ObjectDescriptor> { obj },
        };
        var air = d.Environment.Atmosphere.InitKnown();
        air.AirDensity = 1.25f; air.Temperature = 20f; air.Pressure = 1f;
        return d;
    }

    static DiscoveryFrame DiscoveryMinimal() =>
        Discovery(new ObjectDescriptor { Name = "obj", Type = ObjectType.Vehicle, Primary = true });

    static DiscoveryFrame DiscoveryChannels()
    {
        var obj = new ObjectDescriptor
        {
            Name = "demo_car", Type = ObjectType.Vehicle,
            TypeLabel = "conformance car", Primary = true,
            Channels = new List<ChannelDescriptor>
            {
                new ChannelDescriptor { Id = 0, Name = "engine.rpm", Unit = "rpm",
                                        Description = "Engine revolutions per minute",
                                        Type = ChannelType.Number },
                new ChannelDescriptor { Id = 1, Name = "vehicle.speed", Unit = "m/s",
                                        Type = ChannelType.Number },
                new ChannelDescriptor { Id = 2, Name = "lights.headlights", Unit = "",
                                        Type = ChannelType.Boolean },
            },
        };
        var rpm = obj.Channels[0].InitRange();
        rpm.Min = 0; rpm.Max = 9000;
        obj.Channels[1].InitRange().Max = 30;
        return Discovery(obj);
    }

    static MotionFrame Motion(double timestamp, params MotionObject[] objects) =>
        new MotionFrame { Timestamp = timestamp, Objects = new List<MotionObject>(objects) };

    static MotionFrame MotionPose() => Motion(1.25, new MotionObject
    {
        Name = "obj",
        Position = { X = 1.5f, Y = 0.25f, Z = -3.75f },
        Orientation = { W = 1 },
    });

    static MotionFrame MotionChannels() => Motion(2.5, new MotionObject
    {
        Name = "demo_car",
        Position = { X = -2f, Y = 0.5f, Z = 3f },
        Orientation = { Y = 1 },
        Channels = new List<ChannelValue>
        {
            new ChannelValue { Id = 0, Number = 3250.5f },
            new ChannelValue { Id = 1, Number = 8f },
            new ChannelValue { Id = 2, Boolean = true },
        },
    });

    static MotionFrame MotionMulti() => Motion(3.75,
        new MotionObject { Name = "player_car", Position = { X = 1 }, Orientation = { W = 1 } },
        new MotionObject { Name = "wingman", Position = { X = -1, Y = 0.5f, Z = 2.5f },
                           Orientation = { Y = 1 } });

    static MotionFrame MotionBodyDynamics()
    {
        var obj = new MotionObject
        {
            Name = "obj",
            Position = { X = 0.5f, Y = 1.25f, Z = -2.5f },
            Orientation = { Y = 0.5f, W = 0.875f },
        };
        // accelerometer-style specific force (f = a − g: +1 g on +heave at
        // rest) and C-4 body angular rates — the motion-cueing pair
        var bd = obj.InitBodyDynamics();
        bd.SpecificForce.X = 0.25f; bd.SpecificForce.Y = 9.5f; bd.SpecificForce.Z = -1.5f;
        bd.AngularVelocity.X = -0.125f; bd.AngularVelocity.Y = 0.375f; bd.AngularVelocity.Z = 0.75f;
        return Motion(4.5, obj);
    }

    static DiscoveryFrame DiscoveryGravityVector()
    {
        // tilted session gravity (C-12): the scalar is its magnitude,
        // |(3, −4, 0)| = 5
        var d = Discovery(new ObjectDescriptor { Name = "obj", Type = ObjectType.Spacecraft, Primary = true });
        d.Environment.Gravity = 5f;
        d.Environment.Medium = ContactMedium.Vacuum;
        var g = d.Environment.InitGravityVector();
        g.X = 3f; g.Y = -4f;
        return d;
    }

    static MotionFrame MotionLocalGravity()
    {
        var obj = new MotionObject
        {
            Name = "obj",
            Position = { X = 12f, Y = 0.5f, Z = -4f },
            Orientation = { W = 1 },
        };
        // at rest, level, under a local pull toward −x (C-12): specific
        // force is the reaction, q⁻¹ · (a − g) = (+2, 0, 0)
        var bd = obj.InitBodyDynamics();
        bd.SpecificForce.X = 2f;
        var g = obj.LocalGravity.InitVector();
        g.X = -2f;
        return Motion(5.25, obj);
    }

    static MotionFrame MotionLocalGravityZero()
    {
        // explicitly weightless: a selected all-zero vector, not "not provided"
        var obj = new MotionObject
        {
            Name = "obj",
            Position = { X = 1f, Y = 2f, Z = 3f },
            Orientation = { W = 1 },
        };
        obj.LocalGravity.InitVector();
        return Motion(6.5, obj);
    }

    // One descriptor set split into two pages of generation 3: page 0 carries
    // the object's identity and its first channel; page 1 lists the object
    // by name only, with a session text channel (and its value) and a
    // streamed channel.
    static DiscoveryFrame DiscoveryPaged(ushort page)
    {
        var obj = page == 0
            ? new ObjectDescriptor
            {
                Name = "demo_car", Type = ObjectType.Vehicle,
                TypeLabel = "conformance car", Primary = true,
                Channels = new List<ChannelDescriptor>
                {
                    new ChannelDescriptor { Id = 0, Name = "engine.rpm", Unit = "rpm",
                                            Type = ChannelType.Number },
                },
            }
            : new ObjectDescriptor
            {
                Name = "demo_car",
                Channels = new List<ChannelDescriptor>
                {
                    new ChannelDescriptor { Id = 1, Name = "session.track", Unit = "",
                                            Type = ChannelType.Text },
                    new ChannelDescriptor { Id = 2, Name = "vehicle.speed", Unit = "m/s",
                                            Type = ChannelType.Number },
                },
                Texts = new List<TextValue> { new TextValue { Id = 1, Text = "Test Ring" } },
            };
        var d = Discovery(obj);
        d.Generation = PagedGeneration;
        d.Page = page;
        d.PageCount = 2;
        return d;
    }

    // One tick of generation 3 in two parts: part 0 carries the pose and the
    // first channel; part 1 lists the object by name with the second channel.
    static MotionFrame MotionPart(ushort part)
    {
        var obj = part == 0
            ? new MotionObject
            {
                Name = "demo_car",
                Position = { X = 4f, Y = 0.5f, Z = -1f },
                Orientation = { W = 1 },
                Channels = new List<ChannelValue> { new ChannelValue { Id = 0, Number = 4100f } },
            }
            : new MotionObject
            {
                Name = "demo_car",
                Channels = new List<ChannelValue> { new ChannelValue { Id = 2, Number = 12.5f } },
            };
        var m = Motion(7.75, obj);
        m.Generation = PagedGeneration;
        m.Part = part;
        m.PartCount = 2;
        return m;
    }

    // A sender that names its instance for session pickers and pinning
    // (implementation-udp.md § Senders and sessions).
    static DiscoveryFrame DiscoveryIdentity()
    {
        var d = Discovery(new ObjectDescriptor { Name = "obj", Type = ObjectType.Aircraft, Primary = true });
        d.Environment.Atmosphere.SetNoAtmosphere(); // a game without an atmosphere model
        d.InstanceLabel = "Rig 2";
        d.Host = "sim-pc";
        return d;
    }

    // A wheel that measures drive torque but not brake pressure: the
    // per-field optionality unions select value and stay noBrakePressure.
    static MotionFrame MotionWheel()
    {
        var obj = new MotionObject
        {
            Name = "obj",
            Position = { X = 2f, Y = 0.5f, Z = 1f },
            Orientation = { W = 1 },
        };
        var point = new DrivePoint { Id = 0 };
        var w = point.InitWheel();
        w.Contact = ContactMedium.Asphalt;
        // each measurement is a named union: a view over w selects its value
        var rpm = w.Rpm; rpm.Value = 1200f;
        var slip = w.Slip; slip.Value = 0.0625f;
        var compression = w.Compression; compression.Value = 0.03125f;
        var torque = w.Torque; torque.Value = 850f;
        obj.DrivePoints = new List<DrivePoint> { point };
        return Motion(8.25, obj);
    }

    // World-space kinematics instead of body dynamics (the dynamics union).
    static MotionFrame MotionKinematics()
    {
        var obj = new MotionObject
        {
            Name = "obj",
            Position = { X = 1f, Y = 0.5f, Z = -2f },
            Orientation = { W = 1 },
        };
        var k = obj.InitKinematics();
        k.Velocity.X = 0.5f; k.Velocity.Y = -0.25f; k.Velocity.Z = 12f;
        k.Acceleration.X = 0.125f; k.Acceleration.Z = 2.5f;
        return Motion(9.5, obj);
    }

    // Every drive-point descriptor variant with its spec, and a channel
    // whose value type is left unspecified.
    static DiscoveryFrame DiscoveryDrivePoints()
    {
        var wheel = new DrivePointDescriptor { Id = 0, Name = "wheel_fl", Offset = { X = -0.75f, Y = -0.25f, Z = 1.5f } };
        var ws = wheel.InitWheel();
        ws.Radius = 0.3125f; ws.Driven = true; ws.Steered = true;
        var susp = ws.InitSuspension();
        susp.Travel.Min = -0.0625; susp.Travel.Max = 0.125; susp.Stiffness = 65000f; susp.Damping = 5000f;
        var prop = new DrivePointDescriptor { Id = 1, Name = "prop", Offset = { Z = 2f } };
        var ps = prop.InitPropeller(); ps.Diameter = 1.875f; ps.Blades = 3;
        var jet = new DrivePointDescriptor { Id = 2, Name = "jet", Offset = { Z = -3f } };
        jet.SetJet();
        var fan = new DrivePointDescriptor { Id = 3, Name = "fan", Offset = { Y = 1f } };
        fan.InitGeneric().RatedForce = 1500f;
        var obj = new ObjectDescriptor
        {
            Name = "obj", Type = ObjectType.Vehicle, Primary = true,
            DrivePoints = new List<DrivePointDescriptor> { wheel, prop, jet, fan },
            Channels = new List<ChannelDescriptor>
            {
                new ChannelDescriptor { Id = 0, Name = "mod.value", Unit = "" },
            },
        };
        return Discovery(obj);
    }

    // State for each drive-point kind: a wheel measuring everything, a
    // propeller with pitch and thrust, a jet with thrust, a generic point.
    static MotionFrame MotionDrivePoints()
    {
        var obj = new MotionObject
        {
            Name = "obj",
            Position = { X = 3f },
            Orientation = { W = 1 },
        };
        var wheel = new DrivePoint { Id = 0 };
        var w = wheel.InitWheel();
        w.Contact = ContactMedium.Gravel;
        var rpm = w.Rpm; rpm.Value = 900f;
        var slip = w.Slip; slip.Value = 0.125f;
        var compression = w.Compression; compression.Value = 0.0625f;
        var steer = w.SteerAngle; steer.Value = 0.25f;
        var load = w.LoadForce; load.Value = 4200f;
        var damper = w.DamperVelocity; damper.Value = -0.5f;
        var torque = w.Torque; torque.Value = 300f;
        var brake = w.BrakePressure; brake.Value = 250000f;
        var tyre = w.InitTyre(); tyre.Pressure = 180000f; tyre.Temperature = 85f; tyre.Wear = 0.875f;
        var prop = new DrivePoint { Id = 1 };
        var p = prop.InitPropeller(); p.Rpm = 2400f;
        var pe = p.InitExtended(); pe.Pitch = 0.375f; pe.Thrust = 2000f;
        var jet = new DrivePoint { Id = 2 };
        var j = jet.InitJet(); j.Throttle = 0.75f; j.Thrust = 40000f;
        var fan = new DrivePoint { Id = 3 };
        var g = fan.InitGeneric(); g.Engaged = true;
        var f = g.InitForce(); f.Y = 750f;
        obj.DrivePoints = new List<DrivePoint> { wheel, prop, jet, fan };
        return Motion(10.25, obj);
    }

    // A text channel's value in a motion frame rides the TextValue list.
    static MotionFrame MotionTexts()
    {
        var obj = new MotionObject
        {
            Name = "demo_car",
            Position = { X = 1f },
            Orientation = { W = 1 },
            Channels = new List<ChannelValue> { new ChannelValue { Id = 2, Number = 12.5f } },
            Texts = new List<TextValue> { new TextValue { Id = 1, Text = "Pit lane" } },
        };
        return Motion(11.5, obj);
    }

    static EventFrame EventImpact()
    {
        var impact = new Event { Id = 7, Name = "body.impact", Intensity = 0.75f, Duration = 0.25f };
        impact.InitDirection().X = -1;
        return new EventFrame { Timestamp = 0.125, Events = new List<Event> { impact } };
    }
}
}
