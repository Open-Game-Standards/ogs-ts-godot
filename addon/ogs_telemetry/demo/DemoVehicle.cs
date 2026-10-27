#nullable enable
using Godot;

namespace Ogs.Telemetry.Godot.Demo;

/// <summary>Demo vehicle: moves in a circle feeding rpm/speed/headlights
/// channels. Pair with ogs-ts-dotnet's TelemetryReceive.</summary>
public sealed partial class DemoVehicle : Node3D
{
    double _t;
    const float Radius = 10f;
    const float AngularSpeed = 0.8f; // rad/s
    const float MaxRpm = 6500f;

    public override void _Ready()
    {
        // locals throughout: handles are only used while wiring, and this
        // keeps the file free of nullable-field warnings (CS8602/8604)
        var telemetry = GetNode<TelemetryPublisherNode>("%TelemetryPublisher");
        var rpm = telemetry.DeclareChannel(
            "engine.rpm", Ogs.Telemetry.Unit.Rpm, "Demo engine speed", 0, 9000);
        var speed = telemetry.DeclareChannel(
            "vehicle.speed", Ogs.Telemetry.Unit.MetrePerSecond,
            "Speed over ground", 0, 30);
        var headlights = telemetry.DeclareBooleanChannel(
            "lights.headlights", "Headlights on/off");

        telemetry.BindChannelRaw(rpm, () =>
            MaxRpm * (0.5 + 0.5 * Mathf.Sin((float)_t * 2f)));
        // game-native units — the publisher converts on the wire: speed is
        // measured in km/h here (ω·r in m/s × 3.6), declared m/s.
        telemetry.BindChannel(speed, () => AngularSpeed * Radius * 3.6,
            Ogs.Telemetry.Unit.KilometrePerHour);
        telemetry.BindBooleanChannel(headlights, () => true);

        // pose via manual binding (no physics body needed in the demo);
        // the node converts Godot RH/-Z to OGS-TS LH/+Z inside BindPose.
        telemetry.BindPose(
            () => GlobalPosition,
            () => GlobalTransform.Basis.GetRotationQuaternion());
    }

    public override void _Process(double delta)
    {
        _t += delta;
        float yaw = (float)(_t * AngularSpeed);
        var pos = new Vector3(
            Radius * Mathf.Cos(yaw), 0.5f, Radius * Mathf.Sin(yaw));
        GlobalPosition = pos;
        // face the direction of travel (Godot -Z forward)
        var next = new Vector3(
            Radius * Mathf.Cos(yaw + 0.01f), 0.5f, Radius * Mathf.Sin(yaw + 0.01f));
        LookAt(next, Vector3.Up);
    }
}
