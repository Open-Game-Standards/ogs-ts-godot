#nullable enable
using System;
using Godot;
using Core = Ogs.Telemetry;

namespace Ogs.Telemetry.Godot;

/// <summary>
/// OGS-TS publisher for one vehicle/object. Configure transport and
/// cadence in the Inspector, declare channels in _Ready, then bind game
/// values (pull) or push per tick — declarations lock at the first tick.
///
/// Frame conversion (this engine needs it — unlike Unity): Godot is
/// right-handed with −Z forward; OGS-TS is left-handed with +Z forward.
/// The map is "negate Z": position (x, y, −z); for orientation the
/// similarity transform R′ = diag(1,1,−1)·R·diag(1,1,−1) gives the
/// quaternion (x, y, −z, −w) — verified algebraically (a Godot left-yaw
/// lands facing −X in both frames) and self-inverse. Vectors (velocity,
/// direction, force) convert like position.
///
/// Pause (standard CONVENTIONS C-11): the node ticks from the scene tree's
/// ProcessFrame signal, which fires every frame even while the tree is
/// paused, so it keeps streaming and reports the pause on
/// <c>session.paused</c> without making this node (or its children)
/// process during the pause. SceneTree.Paused is detected automatically;
/// games that pause another way call <see cref="SetPaused"/>.
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
    /// +|g| at rest, g being the gravity the stream declares.</summary>
    [Export] public bool SendBodyDynamics = true;

    /// <summary>Declare the project's default 3D gravity
    /// (physics/3d/default_gravity × default_gravity_vector) as the
    /// session gravity at _Ready (standard CONVENTIONS C-12). Off: the
    /// publisher's declared gravity (standard gravity unless set).</summary>
    [ExportGroup("Gravity")]
    [Export] public bool GravityFromPhysics = true;

    /// <summary>Carry the auto-bound body's total gravity (Area3D
    /// overrides included — planet and point gravity) as per-object local
    /// gravity in every motion frame. For scenes where gravity varies by
    /// position; leave off when the project default applies everywhere.
    /// </summary>
    [Export] public bool LocalGravityFromBody = false;

    /// <summary>Report the game as paused (session.paused) while
    /// SceneTree.Paused is set. Games that pause another way call
    /// SetPaused; both sources combine.</summary>
    [ExportGroup("Pause")]
    [Export] public bool AutoDetectPause = true;

    Core.TelemetryPublisher? _publisher;
    bool _manualPaused, _wasPaused;
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
        Publisher.DeclarePause();

        if (GravityFromPhysics)
            Publisher.SetGravity(ToStandard(
                ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3()
                * (float)ProjectSettings.GetSetting("physics/3d/default_gravity").AsDouble()));

        if (AutoBindBody != null)
        {
            var body = AutoBindBody;
            Publisher.BindPose(
                () => ToStandard(body.GlobalPosition),
                () => ToStandard(body.GlobalTransform.Basis.GetRotationQuaternion()));
            if (SendBodyDynamics)
                Publisher.BindBodyDynamics(
                    () => (_deriver.SpecificForce, _deriver.AngularVelocity));
            if (LocalGravityFromBody)
                Publisher.BindLocalGravity(() => BodyGravity(body));
        }

        // not _Process: that stops while the tree is paused (C-11)
        GetTree().ProcessFrame += OnProcessFrame;
    }

    /// <summary>Reports a pause the engine can't see (a game-managed pause
    /// that leaves SceneTree.Paused unset). Combines with AutoDetectPause:
    /// the game is paused if either says so.</summary>
    public void SetPaused(bool paused) => _manualPaused = paused;

    /// <summary>Whether the publisher is currently reporting a pause.</summary>
    public bool Paused => _publisher?.Paused ?? false;

    void OnProcessFrame()
    {
        var pub = _publisher;
        if (pub == null || Engine.IsEditorHint())
            return;
        bool paused = _manualPaused || (AutoDetectPause && GetTree().Paused);
        if (paused != pub.Paused) pub.SetPaused(paused);
        // derived specific force reacts to the gravity the stream declares
        // for this object (C-12)
        _deriver.GravityVector = pub.EffectiveGravity;
        // derivation runs every tick with a pose (not just at publish
        // cadence): engine-measured velocity, quaternion-delta rates.
        // Nothing is measured while paused (C-11); resuming starts the
        // derivation fresh rather than differentiating across the pause
        if (SendBodyDynamics && AutoBindBody != null && !paused)
        {
            if (GodotObject.IsInstanceValid(AutoBindBody))
            {
                var body = AutoBindBody;
                var t = Time.GetTicksMsec() / 1000.0;
                var pos = ToStandard(body.GlobalPosition);
                var quat = ToStandard(body.GlobalTransform.Basis.GetRotationQuaternion());
                if (_wasPaused)
                    _deriver.Reset(pos, quat, t);
                else
                    _deriver.Sample(t, pos, quat, hasVelocity: true,
                                    engineVelocity: ToStandard(body.LinearVelocity));
            }
            else
            {
                // body freed (scene change): next pose starts fresh
                _deriver.Reset(default, System.Numerics.Quaternion.Identity, 0);
            }
        }
        _wasPaused = paused;
        pub.Tick(Time.GetTicksMsec() / 1000.0);
    }

    public override void _ExitTree()
    {
        if (_tornDown) return;
        _tornDown = true;
        if (!Engine.IsEditorHint())
            GetTree().ProcessFrame -= OnProcessFrame;
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
                               float pressure, Ogs.Telemetry.Schema.ContactMedium medium)
        => Publisher.SetEnvironment(gravity, airDensity, temperature, pressure, medium);

    /// <summary>As above, with gravity as a Godot world-frame vector
    /// (m/s², pointing the way gravity pulls; standard CONVENTIONS C-12).
    /// </summary>
    public void SetEnvironment(Vector3 gravity, float airDensity, float temperature,
                               float pressure, Ogs.Telemetry.Schema.ContactMedium medium)
        => Publisher.SetEnvironment(ToStandard(gravity), airDensity, temperature, pressure, medium);

    /// <summary>Updates only the session gravity (Godot world frame).
    /// GravityFromPhysics declares the project default at _Ready.</summary>
    public void SetGravity(Vector3 gravity) => Publisher.SetGravity(ToStandard(gravity));

    /// <summary>Binds this object's local gravity (Godot world frame, m/s²,
    /// pointing the way gravity pulls), carried in every motion frame while
    /// bound and fed to the body-dynamics derivation; null falls back to
    /// the session gravity. LocalGravityFromBody binds the auto-bound
    /// body's total gravity.</summary>
    public void BindLocalGravity(Func<Vector3>? gravity)
        => Publisher.BindLocalGravity(
            gravity is null ? null : () => ToStandard(gravity()));

    // Area3D overrides included; a freed or space-less body reads the
    // project default rather than a stale value
    System.Numerics.Vector3 BodyGravity(RigidBody3D body)
    {
        if (GodotObject.IsInstanceValid(body) &&
            PhysicsServer3D.BodyGetDirectState(body.GetRid()) is { } state)
            return ToStandard(state.TotalGravity);
        return _publisher?.EnvironmentGravity ?? default;
    }

    // ---- bindings (pull model) ----

    /// <summary>Binds pose getters in Godot's world frame; conversion to
    /// the OGS-TS frame happens inside the lambdas.</summary>
    public void BindPose(Func<Vector3>? position, Func<Quaternion>? orientation)
        => Publisher.BindPose(
            position is null ? null : () => ToStandard(position()),
            orientation is null ? null : () => ToStandard(orientation()));

    /// <summary>Binds a vector-valued getter (velocity, force direction …)
    /// already in OGS-TS frame terms — convert with ToStandard at the
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

    // ---- Godot → OGS-TS frame conversion ----

    /// <summary>Godot (RH, −Z fwd) → OGS-TS (LH, +Z fwd): negate Z.</summary>
    public static System.Numerics.Vector3 ToStandard(Vector3 v) =>
        new(v.X, v.Y, -v.Z);

    /// <summary>Godot quaternion → OGS-TS quaternion: (x, y, −z, −w).
    /// Exact similarity transform R′ = M·R·M with M = diag(1,1,−1);
    /// norm-preserving and self-inverse.</summary>
    public static System.Numerics.Quaternion ToStandard(Quaternion q) =>
        new(q.X, q.Y, -q.Z, -q.W);

    /// <summary>OGS-TS → Godot (same involutions).</summary>
    public static Vector3 ToGodot(System.Numerics.Vector3 v) =>
        new((float)v.X, (float)v.Y, (float)-v.Z);

    public static Quaternion ToGodot(System.Numerics.Quaternion q) =>
        new((float)q.X, (float)q.Y, (float)-q.Z, (float)-q.W);
}
