#nullable enable
using System;

namespace Ogsa.Telemetry
{

/// <summary>Frame type byte in the OMT1 envelope (see implementation-udp.md).</summary>
public enum FrameType : byte
{
    Discovery = 0x00,
    Motion = 0x01,
    Event = 0x02,
}

/// <summary>
/// The 8-byte OMT1 envelope plus Cap'n Proto message framing.
/// Layout: bytes 0-3 magic "OMT1", byte 4 frame type, bytes 5-7 reserved
/// (send 0, ignore on receive), byte 8+ the Cap'n Proto message in standard
/// segment-table encoding (word-aligned, readable in place).
/// </summary>
public static class Envelope
{
    public const int HeaderSize = 8;
    static readonly byte[] Magic = { (byte)'O', (byte)'M', (byte)'T', (byte)'1' };

    public static byte[] Wrap(FrameType type, ReadOnlySpan<byte> capnpMessage)
    {
        var d = new byte[HeaderSize + capnpMessage.Length];
        Magic.AsSpan().CopyTo(d);
        d[4] = (byte)type;
        capnpMessage.CopyTo(d.AsSpan(HeaderSize));
        return d;
    }

    /// <summary>Checks magic; frame types are advisory — unknown types yield
    /// false (receivers silently skip foreign/future frames). Takes a
    /// Memory so the payload slice is zero-copy when the caller owns the
    /// buffer (byte[] converts implicitly).</summary>
    public static bool TryUnwrap(ReadOnlyMemory<byte> datagram, out FrameType type, out ReadOnlyMemory<byte> capnpMessage)
    {
        type = default;
        capnpMessage = default;
        if (datagram.Length < HeaderSize) return false;
        var span = datagram.Span;
        if (!span.Slice(0, 4).SequenceEqual(Magic)) return false;
        var t = (FrameType)span[4];
        // values are contiguous 0..Event — a range check avoids
        // Enum.IsDefined's per-datagram boxing and is netstandard2.1-safe
        if (t > FrameType.Event) return false;
        type = t;
        capnpMessage = datagram.Slice(HeaderSize);
        return true;
    }

    /// <summary>Frames a single-segment Cap'n Proto message in the standard
    /// segment-table format: one u32 (segment count − 1 = 0) and one u32
    /// (segment size in words) — exactly one word, per canonical capnproto —
    /// then the segment. Words are written little-endian by shifts, never
    /// through a host-endian memory view.</summary>
    public static byte[] EncodeMessage(ulong[] segment)
    {
        var buf = new byte[(1 + segment.Length) * 8];
        WriteMessage(buf, 0, segment);
        return buf;
    }

    /// <summary>Full datagram in one allocation: the 8-byte OMT1 header, then
    /// <see cref="EncodeMessage"/>'s bytes. Byte-identical to
    /// <c>Wrap(type, EncodeMessage(segment))</c>.</summary>
    public static byte[] EncodeDatagram(FrameType type, ulong[] segment)
    {
        var buf = new byte[HeaderSize + (1 + segment.Length) * 8];
        Magic.AsSpan().CopyTo(buf);
        buf[4] = (byte)type;
        WriteMessage(buf, HeaderSize, segment);
        return buf;
    }

    static void WriteMessage(byte[] buf, int offset, ulong[] segment)
    {
        WriteWord(buf, offset, (ulong)(uint)segment.Length << 32); // count-1 = 0, size
        for (int i = 0; i < segment.Length; i++)
            WriteWord(buf, offset + 8 * (1 + i), segment[i]);
    }

    static void WriteWord(byte[] buf, int offset, ulong word)
    {
        for (int i = 0; i < 8; i++)
            buf[offset + i] = (byte)(word >> (8 * i));
    }
}
}
