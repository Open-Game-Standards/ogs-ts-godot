#!/usr/bin/env bash
# sync-core.sh — vendor the engine-agnostic core from ../ogsa-ts-dotnet.
# Run from the repo root after pulling upstream changes: ./sync-core.sh
# The core is plain C# source with no package dependencies: nothing to add
# to the game's csproj.
set -euo pipefail
SRC="../ogsa-ts-dotnet/src/Ogsa.Telemetry"
DST="addon/ogsa_telemetry/Core"

changed=0
for f in "$SRC"/*.cs; do
  if ! cmp -s "$f" "$DST/$(basename "$f")"; then
    cp "$f" "$DST/"
    echo "updated: $DST/$(basename "$f")"
    changed=1
  fi
done
for f in "$SRC"/Schema/*.cs; do
  if ! cmp -s "$f" "$DST/Schema/$(basename "$f")"; then
    cp "$f" "$DST/Schema/"
    echo "updated: $DST/Schema/$(basename "$f")"
    changed=1
  fi
done
# Mirror deletions: a vendored source whose upstream file is gone (moved
# or renamed in ogsa-ts-dotnet) is removed.
for f in "$DST"/*.cs "$DST"/Schema/*.cs; do
  [ -f "$f" ] || continue
  if [ ! -f "$SRC/${f#"$DST"/}" ]; then
    rm -f "$f"
    echo "removed: $f"
    changed=1
  fi
done
# guard against Unity-style meta leftovers (Godot uses none)
find "$DST" -name "*.meta" -delete
if [ "$changed" -eq 0 ]; then echo "core up to date"; fi
