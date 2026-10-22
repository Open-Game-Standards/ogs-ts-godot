#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace Ogsa.Telemetry.Godot;

/// <summary>
/// Cross-engine conformance check: run this scene once per engine plugin.
/// Validates (1) the vendored core decodes the standard repo's golden
/// datagrams to the manifest-declared semantics, and (2) this engine's
/// frame conversion against hand-derived vectors (Godot RH/−Z → OGSA-TS
/// LH/+Z). Prints PASS/FAIL per check and exits non-zero on failure.
///
/// Set CorpusDir to the standard repo's conformance/ folder (absolute
/// path is fine) and run the scene (F6).
/// </summary>
public sealed partial class ConformanceCheck : Node
{
    [Export] public string CorpusDir =
        "../OGSA-Telemetry-Standard/conformance";

    int _pass;
    int _fail;

    void Check(string name, bool ok, string detail = "")
    {
        if (ok) _pass++;
        else _fail++;
        GD.Print($"{(ok ? "PASS" : "FAIL")} {name}{(ok || detail == "" ? "" : $" — {detail}")}");
    }

    public override void _Ready()
    {
        FrameConversionChecks();
        CorpusChecks();
        GD.Print($"conformance: {_pass} passed, {_fail} failed");
        GetTree().Quit(_fail == 0 ? 0 : 1);
    }

    void FrameConversionChecks()
    {
        // position: negate Z
        var p = TelemetryPublisherNode.ToStandard(new Vector3(1f, 2f, 3f));
        Check("frame: position negates Z",
            p.X == 1f && p.Y == 2f && p.Z == -3f);

        // identity → (0,0,0,-1) (≡ identity rotation; q ≡ −q)
        var qi = TelemetryPublisherNode.ToStandard(Quaternion.Identity);
        Check("frame: identity quaternion",
            qi.X == 0 && qi.Y == 0 && qi.Z == 0 && System.Math.Abs(qi.W) == 1f);

        // Godot yaw-left 90°: q=(0,√½,0,√½); standard result must face −X
        var yaw = TelemetryPublisherNode.ToStandard(
            new Quaternion(0f, 0.70710678118f, 0f, 0.70710678118f));
        var fwd = System.Numerics.Vector3.Transform(
            new System.Numerics.Vector3(0, 0, 1), yaw);
        Check("frame: yaw-left faces -X",
            fwd.X < -0.999 && System.Math.Abs(fwd.Y) < 1e-6
                          && System.Math.Abs(fwd.Z) < 1e-6,
            $"got ({fwd.X:F3},{fwd.Y:F3},{fwd.Z:F3})");

        // unit norm preserved
        Check("frame: quaternion norm preserved",
            System.Math.Abs(yaw.Length() - 1.0) < 1e-6);

        // self-inverse roundtrip
        var back = TelemetryPublisherNode.ToGodot(yaw);
        Check("frame: conversion self-inverse",
            System.Math.Abs(back.X - 0f) < 1e-6
            && System.Math.Abs(back.Y - 0.70710678118f) < 1e-6
            && System.Math.Abs(back.Z - 0f) < 1e-6
            && System.Math.Abs(back.W - 0.70710678118f) < 1e-6);
    }

    void CorpusChecks()
    {
        var dir = FindCorpusDir();
        if (dir is null)
        {
            GD.Print($"conformance: corpus not found (tried '{CorpusDir}', " +
                $"OGSA_TS_CONFORMANCE env var, sibling of the project, and " +
                $"parents of the project root) — set CorpusDir in the Inspector " +
                $"to the standard repo's conformance/ folder");
            _fail++;
            return;
        }

        var manifest = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(dir, "manifest.json"))).RootElement;
        Check("corpus: manifest schemaVersion 1",
            manifest.GetProperty("schemaVersion").GetInt32() == 1);

        foreach (var v in manifest.GetProperty("vectors").EnumerateArray())
        {
            var id = v.GetProperty("id").GetString()!;
            var file = v.GetProperty("file").GetString()!;
            var hex = string.Concat(
                File.ReadAllLines(Path.Combine(dir, file))
                    .Select(l => l.Trim()));
            var bytes = Convert.FromHexString(hex);

            if (!Ogsa.Telemetry.Envelope.TryUnwrap(
                    bytes, out var type, out var payload))
            {
                Check($"corpus: {id} envelope", false, "TryUnwrap failed");
                continue;
            }
            var root = Capnp.DeserializerState.CreateRoot(
                Ogsa.Telemetry.Envelope.DecodeMessage(payload.Span));
            var failure = type switch
            {
                Ogsa.Telemetry.FrameType.Discovery =>
                    CheckDiscovery(id, CapnpGen.DiscoveryFrame.READER.create(root)),
                Ogsa.Telemetry.FrameType.Motion =>
                    CheckMotion(id, CapnpGen.MotionFrame.READER.create(root)),
                Ogsa.Telemetry.FrameType.Event =>
                    CheckEvent(id, CapnpGen.EventFrame.READER.create(root)),
                _ => "unknown frame type",
            };
            Check($"corpus: {id} decode", failure is null, failure ?? "");
        }
    }

    // Per-vector semantics mirroring ogsa-ts-dotnet's ConformanceVectorTests
    // (manifest-declared values; floats are float32-exact, so == is exact).
    //
    // Each vector is a list of (field path, condition) pairs rather than one
    // chained `&&` expression: on failure the field path that didn't match
    // is reported, instead of a single collapsed "FAIL" that gives no clue
    // which of 10-20 clauses was the culprit. Conditions are lambdas (not
    // plain bools) so they stay lazily evaluated in order, same as `&&`
    // short-circuiting — a count mismatch is reported cleanly instead of
    // the next clause's out-of-range index access throwing.

    /// <summary>Returns the field path of the first failing check, or null
    /// if all passed. A throwing check (e.g. an out-of-range index after an
    /// earlier count mismatch) is reported as a failure at that field path
    /// instead of crashing the whole conformance run.</summary>
    static string? FirstFailure(params (string field, Func<bool> ok)[] checks)
    {
        foreach (var (field, ok) in checks)
        {
            bool passed;
            try { passed = ok(); }
            catch (Exception ex) { return $"{field} (threw {ex.GetType().Name}: {ex.Message})"; }
            if (!passed) return field;
        }
        return null;
    }

    /// <summary>Corpus location order: explicit CorpusDir (Inspector),
    /// OGSA_TS_CONFORMANCE env var, sibling of the game project, then
    /// walking up from the project root (covers projects nested anywhere
    /// inside the alliance checkout).</summary>
    string? FindCorpusDir()
    {
        if (Directory.Exists(CorpusDir)) return CorpusDir;
        var env = System.Environment.GetEnvironmentVariable("OGSA_TS_CONFORMANCE");
        if (!string.IsNullOrEmpty(env) && Directory.Exists(env)) return env;

        string? dir = ProjectSettings.GlobalizePath("res://");
        while (!string.IsNullOrEmpty(dir))
        {
            var sibling = Path.Combine(
                Path.GetDirectoryName(dir.TrimEnd(
                    Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? "",
                "OGSA-Telemetry-Standard", "conformance");
            if (File.Exists(Path.Combine(sibling, "manifest.json"))) return sibling;
            var nested = Path.Combine(dir, "OGSA-Telemetry-Standard", "conformance");
            if (File.Exists(Path.Combine(nested, "manifest.json"))) return nested;

            var parent = Path.GetDirectoryName(dir.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.IsNullOrEmpty(parent) || parent == dir) break;
            dir = parent;
        }
        return null;
    }

    static string? CheckDiscovery(string id, CapnpGen.DiscoveryFrame.READER d) => id switch
    {
        "discovery-minimal" => FirstFailure(
            ("schemaVersion", () => d.SchemaVersion == 1),
            ("gameName", () => d.GameName == "conformance"),
            ("sessionStartUnixUs", () => d.SessionStartUnixUs == 1_730_000_000_000_000L),
            ("objects.count", () => d.Objects.Count == 1),
            ("objects[0].name", () => d.Objects[0].Name == "obj"),
            ("objects[0].type", () => d.Objects[0].Type == CapnpGen.ObjectType.vehicle),
            ("objects[0].primary", () => d.Objects[0].Primary),
            ("objects[0].channels.count", () => d.Objects[0].Channels.Count == 0)),
        "discovery-channels" => FirstFailure(
            ("schemaVersion", () => d.SchemaVersion == 1),
            ("gameName", () => d.GameName == "conformance"),
            ("objects.count", () => d.Objects.Count == 1),
            ("objects[0].name", () => d.Objects[0].Name == "demo_car"),
            ("objects[0].typeLabel", () => d.Objects[0].TypeLabel == "conformance car"),
            ("objects[0].channels.count", () => d.Objects[0].Channels.Count == 3),
            ("channels[0].id", () => d.Objects[0].Channels[0].Id == 0),
            ("channels[0].name", () => d.Objects[0].Channels[0].Name == "engine.rpm"),
            ("channels[0].unit", () => d.Objects[0].Channels[0].Unit == "rpm"),
            ("channels[0].range.min", () => d.Objects[0].Channels[0].Range.Min == 0f),
            ("channels[0].range.max", () => d.Objects[0].Channels[0].Range.Max == 9000f),
            ("channels[1].id", () => d.Objects[0].Channels[1].Id == 1),
            ("channels[1].name", () => d.Objects[0].Channels[1].Name == "vehicle.speed"),
            ("channels[1].unit", () => d.Objects[0].Channels[1].Unit == "m/s"),
            ("channels[1].range.min", () => d.Objects[0].Channels[1].Range.Min == 0f),
            ("channels[1].range.max", () => d.Objects[0].Channels[1].Range.Max == 30f),
            ("channels[2].id", () => d.Objects[0].Channels[2].Id == 2),
            ("channels[2].name", () => d.Objects[0].Channels[2].Name == "lights.headlights"),
            ("channels[2].unit", () => d.Objects[0].Channels[2].Unit == "")),
        _ => "unknown vector id",
    };

    static string? CheckMotion(string id, CapnpGen.MotionFrame.READER m) => id switch
    {
        "motion-pose" => FirstFailure(
            ("timestamp", () => m.Timestamp == 1.25),
            ("objects.count", () => m.Objects.Count == 1),
            ("objects[0].name", () => m.Objects[0].Name == "obj"),
            ("objects[0].position.x", () => m.Objects[0].Position.X == 1.5f),
            ("objects[0].position.y", () => m.Objects[0].Position.Y == 0.25f),
            ("objects[0].position.z", () => m.Objects[0].Position.Z == -3.75f),
            ("objects[0].orientation.x", () => m.Objects[0].Orientation.X == 0f),
            ("objects[0].orientation.y", () => m.Objects[0].Orientation.Y == 0f),
            ("objects[0].orientation.z", () => m.Objects[0].Orientation.Z == 0f),
            ("objects[0].orientation.w", () => m.Objects[0].Orientation.W == 1f)),
        "motion-channels" => FirstFailure(
            ("timestamp", () => m.Timestamp == 2.5),
            ("objects.count", () => m.Objects.Count == 1),
            ("objects[0].name", () => m.Objects[0].Name == "demo_car"),
            ("objects[0].position.x", () => m.Objects[0].Position.X == -2f),
            ("objects[0].position.y", () => m.Objects[0].Position.Y == 0.5f),
            ("objects[0].position.z", () => m.Objects[0].Position.Z == 3f),
            ("objects[0].orientation.y", () => m.Objects[0].Orientation.Y == 1f),
            ("objects[0].channels.count", () => m.Objects[0].Channels.Count == 3),
            ("channels[0].id", () => m.Objects[0].Channels[0].Id == 0),
            ("channels[0].which", () => m.Objects[0].Channels[0].which == CapnpGen.ChannelValue.WHICH.Number),
            ("channels[0].number", () => m.Objects[0].Channels[0].Number == 3250.5f),
            ("channels[1].id", () => m.Objects[0].Channels[1].Id == 1),
            ("channels[1].number", () => m.Objects[0].Channels[1].Number == 8f),
            ("channels[2].id", () => m.Objects[0].Channels[2].Id == 2),
            ("channels[2].which", () => m.Objects[0].Channels[2].which == CapnpGen.ChannelValue.WHICH.Boolean),
            ("channels[2].boolean", () => m.Objects[0].Channels[2].Boolean)),
        "motion-multi" => FirstFailure(
            ("timestamp", () => m.Timestamp == 3.75),
            ("objects.count", () => m.Objects.Count == 2),
            ("objects[0].name", () => m.Objects[0].Name == "player_car"),
            ("objects[0].position.x", () => m.Objects[0].Position.X == 1f),
            ("objects[1].name", () => m.Objects[1].Name == "wingman"),
            ("objects[1].position.x", () => m.Objects[1].Position.X == -1f),
            ("objects[1].position.y", () => m.Objects[1].Position.Y == 0.5f),
            ("objects[1].position.z", () => m.Objects[1].Position.Z == 2.5f)),
        // corpus v2: pose + the bodyDynamics union variant (accelerometer-
        // style specific force, C-4 angular velocity)
        "motion-bodydynamics" => FirstFailure(
            ("timestamp", () => m.Timestamp == 4.5),
            ("objects.count", () => m.Objects.Count == 1),
            ("objects[0].name", () => m.Objects[0].Name == "obj"),
            ("objects[0].which", () => m.Objects[0].which == CapnpGen.MotionObject.WHICH.BodyDynamics),
            ("objects[0].position.x", () => m.Objects[0].Position.X == 0.5f),
            ("objects[0].position.y", () => m.Objects[0].Position.Y == 1.25f),
            ("objects[0].position.z", () => m.Objects[0].Position.Z == -2.5f),
            ("objects[0].orientation.x", () => m.Objects[0].Orientation.X == 0f),
            ("objects[0].orientation.y", () => m.Objects[0].Orientation.Y == 0.5f),
            ("objects[0].orientation.z", () => m.Objects[0].Orientation.Z == 0f),
            ("objects[0].orientation.w", () => m.Objects[0].Orientation.W == 0.875f),
            ("bodyDynamics.specificForce.x", () => m.Objects[0].BodyDynamics.SpecificForce.X == 0.25f),
            ("bodyDynamics.specificForce.y", () => m.Objects[0].BodyDynamics.SpecificForce.Y == 9.5f),
            ("bodyDynamics.specificForce.z", () => m.Objects[0].BodyDynamics.SpecificForce.Z == -1.5f),
            ("bodyDynamics.angularVelocity.x", () => m.Objects[0].BodyDynamics.AngularVelocity.X == -0.125f),
            ("bodyDynamics.angularVelocity.y", () => m.Objects[0].BodyDynamics.AngularVelocity.Y == 0.375f),
            ("bodyDynamics.angularVelocity.z", () => m.Objects[0].BodyDynamics.AngularVelocity.Z == 0.75f)),
        _ => "unknown vector id",
    };

    static string? CheckEvent(string id, CapnpGen.EventFrame.READER e) => id switch
    {
        "event-impact" => FirstFailure(
            ("timestamp", () => e.Timestamp == 0.125),
            ("events.count", () => e.Events.Count == 1),
            ("events[0].id", () => e.Events[0].Id == 7),
            ("events[0].name", () => e.Events[0].Name == "body.impact"),
            ("events[0].intensity", () => e.Events[0].Intensity == 0.75f),
            ("events[0].duration", () => e.Events[0].Duration == 0.25f),
            ("events[0].which", () => e.Events[0].which == CapnpGen.Event.WHICH.Direction),
            ("events[0].direction.x", () => e.Events[0].Direction.X == -1f),
            ("events[0].direction.y", () => e.Events[0].Direction.Y == 0f),
            ("events[0].direction.z", () => e.Events[0].Direction.Z == 0f)),
        _ => "unknown vector id",
    };
}
