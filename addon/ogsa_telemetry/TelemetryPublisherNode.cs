#nullable enable
using System;
using Godot;
using Core = Ogsa.Telemetry;

namespace Ogsa.Telemetry.Godot;

/// <summary>
/// OGSA-TS publisher for one vehicle/object. Configure transport and
/// cadence in the Inspector, declare channels in _Ready, then bind game
/// values (pull) or push per tick — declarations lock at the first tick.
///
/// Frame conversion (this engine needs it — unlike Unity): Godot is
/// right-handed with −Z forward; OGSA-TS is left-handed with +Z forward.
/// The map is "negate Z": position (x, y, −z); for orientation the
/// similarity transform R′ = diag(1,1,−1)·R·diag(1,1,−1) gives the
/// quaternion (x, y, −z, −w) — verified algebraically (a Godot left-yaw
/// lands facing −X in both frames) and self-inverse. Vectors (velocity,
/// direction, force) convert like position.
/// </summary>
public sealed partial class TelemetryPublisherNode : Node3D
{
    [ExportGroup("Transport")]
    [Export] public string MulticastGroup = "239.255.17.17";
    [Export] public int Port = 40123;
    [Export] public int Ttl = 1;

    [ExportGroup("Identity")]
    [Export] public string GameName = "";
    [Export] public string ObjectName = "player";
    [Export] public string TypeLabel = "";

    [ExportGroup("Cadence")]
    [Export] public double MotionHz = 60.0;
    [Export] public double DiscoveryIntervalSeconds = 2.0;

    [ExportGroup("Auto-bind")]
    [Export] public RigidBody3D? AutoBindBody;

    /// <summary>Emit the bodyDynamics variant (specific force + angular
    /// rates — the motion-cueing inputs) derived per tick from the bound
    /// body's pose and its engine-measured LinearVelocity, via the core's
    /// BodyDynamicsDeriver. Accelerometer-style convention: heave reads
    /// +9.80665 at rest.</summary>
    [Export] public bool SendBodyDynamics = true;

    Core.TelemetryPublisher? _publisher;
    Core.TelemetrySender? _sender;
    bool _tornDown;
    readonly Core.BodyDynamicsDeriver _deriver = new();

    /// <summary>The pose→dynamics deriver behind SendBodyDynamics — for
    /// manual binders: sample it per tick in Godot world terms via
    /// SampleBodyDynamics, then Publisher.BindBodyDynamics(() =>
    /// (deriver.SpecificForce, deriver.AngularVelocity)).</summary>
    public Core.BodyDynamicsDeriver BodyDeriver => _deriver;

    /// <summary>The engine-agnostic publisher, constructed on first use
    /// from the exported configuration.</summary>
    public Core.TelemetryPublisher Publisher
    {
        get
        {
            if (_publisher == null)
            {
                if (_tornDown)
                    throw new ObjectDisposedException(nameof(TelemetryPublisherNode));
                _sender = new Core.TelemetrySender(
                    string.IsNullOrEmpty(MulticastGroup) ? "239.255.17.17" : MulticastGroup,
                    Port, Ttl, loopback: true);
                _publisher = new Core.TelemetryPublisher(
                    _sender,
                    string.IsNullOrEmpty(GameName)
                        ? ProjectSettings.GetSetting("application/config/name").AsString()
                        : GameName,
                    MotionHz, DiscoveryIntervalSeconds);
            }
            return _publisher;
        }
    }

    public override void _Ready()
    {
        // never run in the editor process (a scene-open instance must not
        // construct sockets or send — guard even if someone re-adds [Tool])
        if (Engine.IsEditorHint()) return;

        if (!string.IsNullOrEmpty(ObjectName))
            Publisher.DeclareObject(ObjectName, TypeLabel);

        if (AutoBindBody != null)
        {
            var body = AutoBindBody;
            Publisher.BindPose(
                () => ToStandard(body.GlobalPosition),
                () => ToStandard(body.GlobalTransform.Basis.GetRotationQuaternion()));
            if (SendBodyDynamics)
                Publisher.BindBodyDynamics(
                    () => (_deriver.SpecificForce, _deriver.AngularVelocity));
        }
    }

    public override void _Process(double delta)
    {
        var pub = _publisher;
        if (pub == null || Engine.IsEditorHint())
            return;
        // derivation runs every tick with a pose (not just at publish
        // cadence): engine-measured velocity, quaternion-delta rates
        if (SendBodyDynamics && AutoBindBody != null)
        {
            if (GodotObject.IsInstanceValid(AutoBindBody))
            {
                var body = AutoBindBody;
                var t = Time.GetTicksMsec() / 1000.0;
                var pos = ToStandard(body.GlobalPosition);
                var quat = ToStandard(body.GlobalTransform.Basis.GetRotationQuaternion());
                var vel = ToStandard(body.LinearVelocity);
                _deriver.Sample(t, pos, quat, hasVelocity: true, engineVelocity: vel);
            }
            else
            {
                // body freed (scene change): next pose starts fresh
                _deriver.Reset(default, System.Numerics.Quaternion.Identity, 0);
            }
        }
        pub.Tick(Time.GetTicksMsec() / 1000.0);
    }

    public override void _ExitTree()
    {
        if (_tornDown) return;
        _tornDown = true;
        _publisher?.Dispose();
        _sender?.Dispose();
    }

    // ---- declarations (_Ready; locked at first tick) ----

    public Core.ChannelHandle DeclareChannel(string name, Core.Unit unit,
                                             string description = "",
                                             double min = double.NaN,
                                             double max = double.NaN)
        => Publisher.DeclareChannel(name, unit, description, min, max);

    public Core.ChannelHandle DeclareBooleanChannel(string name,
                                                    string description = "")
        => Publisher.DeclareBooleanChannel(name, description);

    public void AddWheel(string name, Vector3 offset, float radius,
                         bool driven, bool steered,
                         float stiffness = 65000f, float damping = 5000f)
        => Publisher.AddWheel(name, ToStandard(offset), radius, driven, steered,
                              stiffness, damping);

    /// <summary>Declares or updates the ambient environment carried in
    /// discovery (re-sent on change — e.g. entering a space level: gravity
    /// 0, medium vacuum). Callable before the first tick and afterwards.
    /// </summary>
    public void SetEnvironment(float gravity, float airDensity, float temperature,
                               float pressure, Ogsa.Telemetry.Schema.ContactMedium medium)
        => Publisher.SetEnvironment(gravity, airDensity, temperature, pressure, medium);

    // ---- bindings (pull model) ----

    /// <summary>Binds pose getters in Godot's world frame; conversion to
    /// the OGSA-TS frame happens inside the lambdas.</summary>
    public void BindPose(Func<Vector3>? position, Func<Quaternion>? orientation)
        => Publisher.BindPose(
            position is null ? null : () => ToStandard(position()),
            orientation is null ? null : () => ToStandard(orientation()));

    /// <summary>Binds a vector-valued getter (velocity, force direction …)
    /// already in OGSA-TS frame terms — convert with ToStandard at the
    /// call site; exposed here for symmetry with the Unity wrapper.</summary>
    public void BindChannel(Core.ChannelHandle channel, Func<double> source,
                            Core.Unit sourceUnit)
        => Publisher.BindChannel(channel, source, sourceUnit);

    public void BindChannelRaw(Core.ChannelHandle channel, Func<double> source)
        => Publisher.BindChannelRaw(channel, source);

    public void BindBooleanChannel(Core.ChannelHandle channel, Func<bool> source)
        => Publisher.BindBooleanChannel(channel, source);

    // ---- push model ----

    public void SetChannel(Core.ChannelHandle channel, double value)
        => Publisher.SetChannel(channel, value);

    public void SetChannel(Core.ChannelHandle channel, bool value)
        => Publisher.SetChannel(channel, value);

    public void FireEvent(string name, float intensity = 1f, float duration = 0f,
                          Vector3 direction = default)
        => Publisher.FireEvent(name, intensity, duration,
                               new System.Numerics.Vector3(
                                   direction.X, direction.Y, -direction.Z));

    // ---- Godot → OGSA-TS frame conversion ----

    /// <summary>Godot (RH, −Z fwd) → OGSA-TS (LH, +Z fwd): negate Z.</summary>
    public static System.Numerics.Vector3 ToStandard(Vector3 v) =>
        new(v.X, v.Y, -v.Z);

    /// <summary>Godot quaternion → OGSA-TS quaternion: (x, y, −z, −w).
    /// Exact similarity transform R′ = M·R·M with M = diag(1,1,−1);
    /// norm-preserving and self-inverse.</summary>
    public static System.Numerics.Quaternion ToStandard(Quaternion q) =>
        new(q.X, q.Y, -q.Z, -q.W);

    /// <summary>OGSA-TS → Godot (same involutions).</summary>
    public static Vector3 ToGodot(System.Numerics.Vector3 v) =>
        new((float)v.X, (float)v.Y, (float)-v.Z);

    public static Quaternion ToGodot(System.Numerics.Quaternion q) =>
        new((float)q.X, (float)q.Y, (float)-q.Z, (float)-q.W);
}
