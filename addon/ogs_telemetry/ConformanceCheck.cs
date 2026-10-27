#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace Ogs.Telemetry.Godot;

/// <summary>
/// Cross-engine conformance check: run this scene once per engine plugin.
/// Validates (1) the vendored core, running on this engine's .NET runtime,
/// encodes the standard repo's golden datagrams byte for byte, and (2) this engine's
/// frame conversion against hand-derived vectors (Godot RH/−Z → OGS-TS
/// LH/+Z), and (3) that the publisher node keeps streaming with
/// session.paused set while the scene tree is paused (CONVENTIONS C-11).
/// Prints PASS/FAIL per check and exits non-zero on failure.
///
/// Set CorpusDir to the standard repo's conformance/ folder (absolute
/// path is fine) and run the scene (F6).
/// </summary>
public sealed partial class ConformanceCheck : Node
{
    [Export] public string CorpusDir =
        "../OGS-Telemetry-Standard/conformance";

    int _pass;
    int _fail;

    void Check(string name, bool ok, string detail = "")
    {
        if (ok) _pass++;
        else _fail++;
        GD.Print($"{(ok ? "PASS" : "FAIL")} {name}{(ok || detail == "" ? "" : $" — {detail}")}");
    }

    public override async void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; // the pause check pauses the tree
        FrameConversionChecks();
        CorpusChecks();
        await PauseChecks();
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
                $"OGS_TS_CONFORMANCE env var, sibling of the project, and " +
                $"parents of the project root) — set CorpusDir in the Inspector " +
                $"to the standard repo's conformance/ folder");
            _fail++;
            return;
        }

        var manifest = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(dir, "manifest.json"))).RootElement;
        Check("corpus: manifest schemaVersion 1",
            manifest.GetProperty("schemaVersion").GetInt32() == 1);
        var files = manifest.GetProperty("vectors").EnumerateArray().ToDictionary(
            v => v.GetProperty("id").GetString()!,
            v => v.GetProperty("file").GetString()!);

        // Encode direction: the vendored core, running on this engine's
        // .NET runtime, must reproduce every golden datagram byte for byte.
        foreach (var vector in Ogs.Telemetry.ConformanceVectors.All())
        {
            if (!files.TryGetValue(vector.Id, out var file))
            {
                Check($"corpus: {vector.Id} encode", false, "not in the manifest");
                continue;
            }
            var expected = Convert.FromHexString(string.Concat(
                File.ReadAllLines(Path.Combine(dir, file)).Select(l => l.Trim())));
            var actual = vector.Encode();
            Check($"corpus: {vector.Id} encode",
                actual.AsSpan().SequenceEqual(expected),
                $"{actual.Length} bytes encoded, corpus has {expected.Length}");
        }
    }

    /// <summary>Pauses the tree under a live publisher node and counts
    /// what reaches a multicast listener: motion must keep flowing, with
    /// the node reporting the pause. A host without multicast loopback
    /// delivers nothing at all, which is reported as a skip, not a pass.</summary>
    async Task PauseChecks()
    {
        const int port = 41399;
        using var rx = new UdpClient();
        rx.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        rx.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        rx.JoinMulticastGroup(IPAddress.Parse("239.255.17.17"));

        var node = new TelemetryPublisherNode { Port = port, ObjectName = "pause_check" };
        AddChild(node);
        // GravityFromPhysics: the project default is the session gravity (C-12)
        var projectGravity = ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3()
            * (float)ProjectSettings.GetSetting("physics/3d/default_gravity").AsDouble();
        Check("gravity: project default declared as session gravity",
            node.Publisher.EnvironmentGravity == TelemetryPublisherNode.ToStandard(projectGravity),
            $"declared {node.Publisher.EnvironmentGravity}");
        GetTree().Paused = true;
        await ToSignal(GetTree().CreateTimer(0.5, processAlways: true),
                       SceneTreeTimer.SignalName.Timeout);

        int total = 0, motion = 0;
        var from = new IPEndPoint(IPAddress.Any, 0);
        while (rx.Available > 0)
        {
            var d = rx.Receive(ref from);
            total++;
            if (d.Length > 4 && d[4] == (byte)Ogs.Telemetry.FrameType.Motion) motion++;
        }
        Check("pause: node reports session.paused", node.Paused);
        if (total == 0)
            GD.Print("SKIP pause: streaming (no multicast loopback on this host)");
        else
            Check("pause: motion keeps streaming while the tree is paused",
                motion >= 5, $"{motion} motion frames in 0.5 s");

        GetTree().Paused = false;
        node.QueueFree();
    }

    /// <summary>Corpus location order: explicit CorpusDir (Inspector),
    /// OGS_TS_CONFORMANCE env var, sibling of the game project, then
    /// walking up from the project root (covers projects nested anywhere
    /// inside the workspace checkout).</summary>
    string? FindCorpusDir()
    {
        if (Directory.Exists(CorpusDir)) return CorpusDir;
        var env = System.Environment.GetEnvironmentVariable("OGS_TS_CONFORMANCE");
        if (!string.IsNullOrEmpty(env) && Directory.Exists(env)) return env;

        string? dir = ProjectSettings.GlobalizePath("res://");
        while (!string.IsNullOrEmpty(dir))
        {
            var sibling = Path.Combine(
                Path.GetDirectoryName(dir.TrimEnd(
                    Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? "",
                "OGS-Telemetry-Standard", "conformance");
            if (File.Exists(Path.Combine(sibling, "manifest.json"))) return sibling;
            var nested = Path.Combine(dir, "OGS-Telemetry-Standard", "conformance");
            if (File.Exists(Path.Combine(nested, "manifest.json"))) return nested;

            var parent = Path.GetDirectoryName(dir.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.IsNullOrEmpty(parent) || parent == dir) break;
            dir = parent;
        }
        return null;
    }
}
