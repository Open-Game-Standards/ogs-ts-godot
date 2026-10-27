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
    };

    static DiscoveryFrame Discovery(ObjectDescriptor obj) => new DiscoveryFrame
    {
        SchemaVersion = 1,
        GameName = "conformance",
        SessionStartUnixUs = SessionStartUnixUs,
        Environment =
        {
            AirDensity = 1.25f, Temperature = 20f, Pressure = 1f,
            Gravity = 9.5f, Medium = ContactMedium.Asphalt,
        },
        Objects = new List<ObjectDescriptor> { obj },
    };

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
                                        Description = "Engine revolutions per minute" },
                new ChannelDescriptor { Id = 1, Name = "vehicle.speed", Unit = "m/s" },
                new ChannelDescriptor { Id = 2, Name = "lights.headlights", Unit = "" },
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

    static EventFrame EventImpact()
    {
        var impact = new Event { Id = 7, Name = "body.impact", Intensity = 0.75f, Duration = 0.25f };
        impact.InitDirection().X = -1;
        return new EventFrame { Timestamp = 0.125, Events = new List<Event> { impact } };
    }
}
}
