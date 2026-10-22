# ogsa-ts-godot

Godot 4 (.NET) addon for the [OGSA Telemetry Standard (OGSA-TS)](../OGSA-Telemetry-Standard)
— broadcast live motion and event telemetry from a Godot game to motion
platforms, haptic hardware, dashboards, and loggers on the player's local
network, over UDP multicast. Nothing leaves the LAN.

Community & support: [Discord](https://discord.gg/tseF4Q6U99)

Status: **bootstrap, validated** — conformance 12/12 and end-to-end in Godot 4.7.2 .NET (channels=3 ratified units, circle on-wire, |q|=1). `TelemetryPublisherNode`, vendored `Ogsa.Telemetry`
core (the same source tested in ogsa-ts-dotnet), demo scene, and a
conformance checker against the standard's golden corpus. Validated
end-to-end against the reference receiver (`ogsa-ts-dotnet`:
`dotnet run --project src/TelemetryReceive`).

**Built with AI assistance.** Significant portions of this addon
(including the vendored core, the frame-conversion math, and the
conformance checker) were written with AI coding assistance under
human direction and review. Review and test before relying
on it, especially around networking and the frame-conversion math.

## Install (Godot 4 .NET)

1. Copy `addon/ogsa_telemetry/` into your project as `addons/ogsa_telemetry/`.
2. Add the Cap'n Proto runtime to your game project (Godot has a real
   csproj, so no DLL vendoring is needed — NuGet resolves the dependency
   tree):

   ```
   dotnet add package Capnp.Net.Runtime --version 1.3.118
   ```

3. Project Settings → Plugins → enable **OGSA Telemetry (OGSA-TS)**.

## Quick start

Add a `TelemetryPublisherNode` to your vehicle (or instantiate from code),
then wire your game's values in `_Ready`:

```csharp
var telemetry = GetNode<TelemetryPublisherNode>("%TelemetryPublisher");

var rpm   = telemetry.DeclareChannel("engine.rpm", Unit.Rpm, min: 0, max: 9000);
var speed = telemetry.DeclareChannel("vehicle.speed", Unit.MetrePerSecond);

telemetry.BindChannel(speed, () => hud.SpeedKmh, Unit.KilometrePerHour);
telemetry.BindChannelRaw(rpm, () => engine.Rpm);
telemetry.FireEvent("body.impact", intensity: 0.8f, duration: 0.25f);
```

Pose: assign a `RigidBody3D` to **AutoBindBody** (zero code), or
`BindPose(() => GlobalPosition, () => GlobalTransform.Basis.GetRotationQuaternion())`.

## Frame conversion (this engine needs it)

Godot is right-handed with **−Z forward**; OGSA-TS is left-handed with
**+Z forward**. The wrapper converts for you — position negates Z, and
orientation maps `(x, y, z, w) → (x, y, −z, −w)`, the exact similarity
transform `R′ = diag(1,1,−1)·R·diag(1,1,−1)` (norm-preserving,
self-inverse; a Godot left-yaw provably lands facing −X in both frames).
Vectors you pass as event directions are converted the same way. The
conversion is also exposed as `TelemetryPublisherNode.ToStandard` /
`ToGodot` for your own use.

## Conformance check

Open `addons/ogsa_telemetry/ConformanceScene.tscn` and press **F6**. The
checker locates the corpus automatically: exported `CorpusDir` if it
exists, else the `OGSA_TS_CONFORMANCE` env var, else a sibling
`OGSA-Telemetry-Standard/` of the project, else any ancestor (covers
projects nested in the alliance checkout). Every golden vector is
verified against its manifest-declared semantics — names, units,
ranges, positions, orientations, channel values — plus five
hand-derived frame-conversion checks (RH/−Z → LH/+Z).
It validates the vendored core against the golden datagrams and the
frame-conversion math against hand-derived vectors; non-zero exit on
failure. Run this after every core sync.

## Demo

`demo/DemoScene.tscn` — a vehicle circling at radius 10 m feeding
rpm/speed/headlights channels. Press **F5/F6** with
`TelemetryReceive` running beside it.

## Layout

| Path | Contents |
|---|---|
| `addon/ogsa_telemetry/TelemetryPublisherNode.cs` | Godot-facing wrapper: exported config, RH→LH frame conversion, auto-bind |
| `addon/ogsa_telemetry/Core/` | Vendored `Ogsa.Telemetry` core + generated bindings (`sync-core.sh` refreshes from `../ogsa-ts-dotnet`) |
| `addon/ogsa_telemetry/ConformanceCheck.cs` | Corpus + frame-conversion validator |
| `sync-addon.ps1` | Mirror the addon into a Godot project (`-Project <path>`) |
| `addon/ogsa_telemetry/demo/` | Demo vehicle + scene |

## Vendored core discipline

Same as ogsa-ts-unity: core source is copied from ogsa-ts-dotnet (single
source of truth; its multi-target build + 33-test suite are the gate).
After pulling upstream: `./sync-core.sh`. Unlike Unity, Godot needs no
vendored DLLs — NuGet handles Capnp.Net.Runtime in the game csproj.

## Wiring your game's values

Declare channels once in `_Ready` (they lock at the first tick), bind
getters to your node's state, and fire events from signals. Pose:
`AutoBindBody` for physics bodies, or manual `BindPose`.

```csharp
public partial class Vehicle : RigidBody3D
{
    [Export] public float SpeedKmh, EngineRpm;

    TelemetryPublisherNode _telemetry = null!;

    public override void _Ready()
    {
        _telemetry = GetNode<TelemetryPublisherNode>("../TelemetryPublisher");
        var rpm   = _telemetry.DeclareChannel("engine.rpm", Unit.Rpm, min: 0, max: 9000);
        var speed = _telemetry.DeclareChannel("vehicle.speed", Unit.MetrePerSecond);

        _telemetry.BindChannelRaw(rpm, () => EngineRpm);
        _telemetry.BindChannel(speed, () => SpeedKmh, Unit.KilometrePerHour);

        // events from gameplay signals (direction converts RH -> LH for you)
        BodyEntered += body =>
            _telemetry.FireEvent("body.impact", 0.8f, 0.25f, -LinearVelocity.Normalized());
    }
}
```

Pull bindings are sampled at the publisher's rate — no per-frame calls
needed. `SetChannel` exists for push-model timing control.
