#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ogsa.Telemetry.Wire
{

/// <summary>
/// Base of the generated schema writers (<c>Schema/ogsa_telemetry.writers.cs</c>):
/// one struct's data words and pointer slots, held as an object tree until
/// <see cref="CanonicalEncoder"/> lays the whole message out. Pointer slots
/// hold a <see cref="string"/> (Text), a <see cref="WireStruct"/>, or a
/// <see cref="List{T}"/> of WireStructs.
///
/// Pure managed code — no unsafe, no reflection, no host-endian memory
/// views — so it runs unchanged on Mono, IL2CPP (AOT), NativeAOT, ARM and
/// big-endian hosts.
/// </summary>
public abstract class WireStruct
{
    internal readonly ulong[] Data;
    internal readonly object?[] Pointers;

    protected WireStruct(int dataWords, int pointerCount)
    {
        Data = new ulong[dataWords];
        Pointers = new object?[pointerCount];
    }

    // Fields are aligned to their own width, so one never spans two words.
    protected ulong GetBits(int bitOffset, int width)
    {
        ulong mask = width == 64 ? ulong.MaxValue : (1UL << width) - 1;
        return (Data[bitOffset >> 6] >> (bitOffset & 63)) & mask;
    }

    protected void SetBits(int bitOffset, int width, ulong value)
    {
        ulong mask = width == 64 ? ulong.MaxValue : (1UL << width) - 1;
        int shift = bitOffset & 63;
        ref ulong word = ref Data[bitOffset >> 6];
        word = (word & ~(mask << shift)) | ((value & mask) << shift);
    }

    protected bool GetBool(int bitOffset) => GetBits(bitOffset, 1) != 0;
    protected void SetBool(int bitOffset, bool value) => SetBits(bitOffset, 1, value ? 1UL : 0UL);

    protected float GetFloat32(int bitOffset) =>
        new FloatBits { Bits = (uint)GetBits(bitOffset, 32) }.Value;
    protected void SetFloat32(int bitOffset, float value) =>
        SetBits(bitOffset, 32, new FloatBits { Value = value }.Bits);

    protected double GetFloat64(int bitOffset) =>
        BitConverter.Int64BitsToDouble((long)GetBits(bitOffset, 64));
    protected void SetFloat64(int bitOffset, double value) =>
        SetBits(bitOffset, 64, (ulong)BitConverter.DoubleToInt64Bits(value));

    protected object? GetPointer(int index) => Pointers[index];
    protected void SetPointer(int index, object? value) => Pointers[index] = value;

    // BitConverter.SingleToInt32Bits is not in netstandard2.0; an explicit
    // layout reinterprets the value (not memory order), so it is endian-safe.
    [StructLayout(LayoutKind.Explicit)]
    struct FloatBits
    {
        [FieldOffset(0)] public float Value;
        [FieldOffset(0)] public uint Bits;
    }
}

/// <summary>
/// Lays a <see cref="WireStruct"/> tree out as a single-segment Cap'n Proto
/// message in canonical form (Cap'n Proto encoding spec, Canonicalization):
/// objects in preorder, trailing zero data words and null pointers
/// truncated (per struct list: across all elements), zero-sized structs at
/// offset −1. Together with the writers' field-presence rules this is the
/// standard's canonical OGSA encoding: the bytes depend only on content.
/// </summary>
public static class CanonicalEncoder
{
    /// <summary>The message's segment words (no segment table).</summary>
    public static ulong[] Encode(WireStruct root)
    {
        var words = new List<ulong>(64) { 0 };
        WriteStruct(words, root, 0);
        return words.ToArray();
    }

    /// <summary>A complete OMT1 datagram: envelope, one-segment table, words.</summary>
    public static byte[] EncodeDatagram(FrameType type, WireStruct root) =>
        Envelope.EncodeDatagram(type, Encode(root));

    static bool IsNull(object? p) => p is null || (p is IList list && list.Count == 0);

    static int DataWords(WireStruct s)
    {
        int n = s.Data.Length;
        while (n > 0 && s.Data[n - 1] == 0) n--;
        return n;
    }

    static int PointerWords(WireStruct s)
    {
        int n = s.Pointers.Length;
        while (n > 0 && IsNull(s.Pointers[n - 1])) n--;
        return n;
    }

    static void WritePointer(List<ulong> words, object? p, int at)
    {
        if (IsNull(p)) return; // null pointer: the word stays 0
        switch (p)
        {
            case WireStruct s: WriteStruct(words, s, at); break;
            case string text: WriteText(words, text, at); break;
            case IList list: WriteList(words, list, at); break;
            default: throw new InvalidOperationException($"unsupported pointer value {p!.GetType()}");
        }
    }

    static void WriteStruct(List<ulong> words, WireStruct s, int at)
    {
        int dw = DataWords(s), pw = PointerWords(s);
        if (dw == 0 && pw == 0)
        {
            words[at] = StructPointer(-1, 0, 0);
            return;
        }
        int start = words.Count;
        for (int i = 0; i < dw; i++) words.Add(s.Data[i]);
        for (int i = 0; i < pw; i++) words.Add(0);
        words[at] = StructPointer(start - at - 1, dw, pw);
        for (int i = 0; i < pw; i++)
            WritePointer(words, s.Pointers[i], start + dw + i);
    }

    static void WriteText(List<ulong> words, string text, int at)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        int count = bytes.Length + 1; // NUL terminator
        int start = words.Count;
        for (int i = 0; i < (count + 7) / 8; i++) words.Add(0);
        for (int i = 0; i < bytes.Length; i++)
            words[start + i / 8] |= (ulong)bytes[i] << (8 * (i % 8));
        words[at] = ListPointer(start - at - 1, 2, count);
    }

    static void WriteList(List<ulong> words, IList list, int at)
    {
        int n = list.Count, dw = 0, pw = 0;
        for (int k = 0; k < n; k++)
        {
            var e = (WireStruct)list[k]!;
            dw = Math.Max(dw, DataWords(e));
            pw = Math.Max(pw, PointerWords(e));
        }
        int stride = dw + pw;
        int start = words.Count;
        words.Add(((ulong)(uint)n << 2) | ((ulong)dw << 32) | ((ulong)pw << 48)); // tag
        for (int k = 0; k < n; k++)
        {
            var e = (WireStruct)list[k]!;
            for (int i = 0; i < dw; i++) words.Add(e.Data[i]);
            for (int i = 0; i < pw; i++) words.Add(0);
        }
        words[at] = ListPointer(start - at - 1, 7, n * stride);
        for (int k = 0; k < n; k++)
        {
            var e = (WireStruct)list[k]!;
            for (int i = 0; i < pw; i++)
                WritePointer(words, e.Pointers[i], start + 1 + k * stride + dw + i);
        }
    }

    static ulong StructPointer(int offset, int dataWords, int pointerWords) => unchecked(
        (ulong)(uint)(offset << 2) | ((ulong)dataWords << 32) | ((ulong)pointerWords << 48));

    static ulong ListPointer(int offset, int sizeCode, int count) => unchecked(
        1UL | (ulong)(uint)(offset << 2) | ((ulong)sizeCode << 32) | ((ulong)(uint)count << 35));
}
}
