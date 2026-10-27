# ogs-ts-godot working conventions

- Core is vendored under `addon/ogs_telemetry/Core/` from
  `../ogs-ts-dotnet` (single source of truth). Never hand-edit vendored
  files; refresh with `./sync-core.sh`.
- The core must stay C# 9 / netstandard2.1-clean; the dotnet repo's
  multi-target build + tests are the gate.
- The Godot wrapper is the only engine-facing surface: exported config,
  Godot-typed overloads, RigidBody3D auto-bind, _Process tick with
  Time.GetTicksMsec()/1000.0.
- The frame conversion (RH/−Z → LH/+Z: negate Z; quaternion
  (x,y,−z,−w)) lives in TelemetryPublisherNode and must stay covered by
  ConformanceCheck's hand-derived vectors — run it after every core sync.
- Godot test projects consume a *copy* under <project>/addons/ogs_telemetry —
  after changing the addon run ./sync-addon.ps1 -Project <path> so the copy
  never drifts (preserves Godot-generated .uid sidecars).
- testproject/ is the in-repo host project (conformance scene is the main
  scene): ./sync-addon.ps1 -Project ./testproject && dotnet build
  testproject/*.csproj && godot --headless --path testproject — exit 0 means
  every check passed (5 frame + manifest + one encode check per corpus
  vector). CI runs exactly this.
- Product docs describe current state only; history lives in the
  Open-Game-Standards knowledge base (knowledge/README.md bridge).
