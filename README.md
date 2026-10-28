# ogs-ts-godot

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="assets/telemetry-standard-dark.svg">
  <img align="right" width="120" src="assets/telemetry-standard.svg" alt="OGS Telemetry Standard logo">
</picture>

Godot 4 (.NET) addon for the [OGS Telemetry Standard (OGS-TS)](https://github.com/Open-Game-Standards/OGS-Telemetry-Standard)
— broadcast live motion and event telemetry from a Godot game to motion
platforms, haptic hardware, dashboards, and loggers on the player's local
network, over UDP multicast. Nothing leaves the LAN.

Community & support: [Discord](https://discord.gg/tseF4Q6U99) · Development is funded by [sponsorship](https://github.com/sponsors/BrianGilbert)

Status: **validated** — conformance 12/12 and end-to-end in Godot 4.7.2 .NET (channels=3 ratified units, circle on-wire, |q|=1). `TelemetryPublisherNode`, vendored `Ogs.Telemetry`
core (the same source tested in ogs-ts-dotnet), demo scene, and a
conformance checker against the standard's golden corpus. Validated
end-to-end against the reference receiver (`ogs-ts-dotnet`:
`dotnet run --project src/TelemetryReceive`).

**Built with AI assistance.** Significant portions of this addon
(including the vendored core, the frame-conversion math, and the
conformance checker) were written with AI coding assistance under
human direction and review. Review and test before relying
on it, especially around networking and the frame-conversion math.

## Install (Godot 4 .NET)

1. Copy `addon/ogs_telemetry/` into your project as `addons/ogs_telemetry/`.
   The addon is plain C# source with no package dependencies — nothing to
   add to your game's csproj.
2. Project Settings → Plugins → enable **OGS Telemetry (OGS-TS)**.

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

Godot is right-handed with **−Z forward**; OGS-TS is left-handed with
**+Z forward**. The wrapper converts for you — position negates Z, and
orientation maps `(x, y, z, w) → (x, y, −z, −w)`, the exact similarity
transform `R′ = diag(1,1,−1)·R·diag(1,1,−1)` (norm-preserving,
self-inverse; a Godot left-yaw provably lands facing −X in both frames).
Vectors you pass as event directions are converted the same way. The
conversion is also exposed as `TelemetryPublisherNode.ToStandard` /
`ToGodot` for your own use.

## Conformance check

Open `addons/ogs_telemetry/ConformanceScene.tscn` and press **F6**. The
checker locates the corpus automatically: exported `CorpusDir` if it
exists, else the `OGS_TS_CONFORMANCE` env var, else a sibling
`OGS-Telemetry-Standard/` of the project, else any ancestor (covers
projects nested in the workspace checkout). The vendored core encodes
every golden vector on Godot's .NET runtime and must reproduce the
corpus byte for byte, plus five hand-derived frame-conversion checks
(RH/−Z → LH/+Z) and a pause check: with the scene tree paused, a live
publisher node must keep streaming motion frames and report
`session.paused`.
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
| `addon/ogs_telemetry/TelemetryPublisherNode.cs` | Godot-facing wrapper: exported config, RH→LH frame conversion, auto-bind |
| `addon/ogs_telemetry/Core/` | Vendored `Ogs.Telemetry` core + generated bindings (`sync-core.sh` refreshes from `../ogs-ts-dotnet`) |
| `addon/ogs_telemetry/ConformanceCheck.cs` | Corpus + frame-conversion validator |
| `sync-addon.ps1` | Mirror the addon into a Godot project (`-Project <path>`) |
| `addon/ogs_telemetry/demo/` | Demo vehicle + scene |

## Vendored core discipline

Same as ogs-ts-unity: core source is copied from ogs-ts-dotnet (single
source of truth; its multi-target build + test suite are the gate).
After pulling upstream: `./sync-core.sh` (it also removes vendored files
that no longer exist upstream). The core writes Cap'n Proto frames itself
(generated schema writers, canonical OGS encoding), so there is no
runtime library to install.

## Pause

The publisher keeps streaming while the game is paused and reports the
pause on the ratified `session.paused` channel ([CONVENTIONS C-11](https://github.com/Open-Game-Standards/OGS-Telemetry-Standard/blob/main/CONVENTIONS.md#c-11-pause)), so motion
rigs and haptics come to rest instead of freezing mid-move or dropping
the session. `SceneTree.Paused` is detected automatically (`AutoDetectPause`; the node ticks from `SceneTree.ProcessFrame`, so it never processes your scene during a pause);
games that pause another way call `SetPaused(true/false)`. Nothing to wire.

## Gravity

The session gravity in discovery is the project's default 3D gravity
(`GravityFromPhysics`, on by default), converted to the OGS-TS frame,
and the body-dynamics derivation reacts to the same gravity, so heave at
rest reads your project's |g| ([CONVENTIONS C-12](https://github.com/Open-Game-Standards/OGS-Telemetry-Standard/blob/main/CONVENTIONS.md#c-12-gravity)). For scenes where
gravity varies by position (`Area3D` point or planet gravity), turn on
`LocalGravityFromBody`: the auto-bound body's total gravity rides every
motion frame as per-object local gravity. `SetGravity`,
`SetEnvironment(Vector3 …)` and `BindLocalGravity` take Godot world-frame
vectors.

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
