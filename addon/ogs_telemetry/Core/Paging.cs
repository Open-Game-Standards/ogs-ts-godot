#nullable enable
using System;
using System.Collections.Generic;
using Ogs.Telemetry.Schema;
using Ogs.Telemetry.Wire;

namespace Ogs.Telemetry
{

/// <summary>
/// The datagram budget (implementation-udp.md § Datagram budget): every
/// OGS-TS datagram, envelope included, is at most this many bytes.
/// </summary>
public static class DatagramBudget
{
    public const int Bytes = 1400;

    /// <summary>Encoded datagram size of <paramref name="frame"/>, envelope included.</summary>
    public static int SizeOf(FrameType type, WireStruct frame) =>
        CanonicalEncoder.EncodeDatagram(type, frame).Length;
}

/// <summary>
/// Splits a descriptor set into discovery pages that each fit the datagram
/// budget (implementation-udp.md § Paged discovery). Cold path: pages are
/// built once per generation, by measuring candidate pages with the
/// canonical encoder, and then re-sent as they are.
///
/// Layout: objects in the given order (primary first). An object's
/// identity (type, labels, primary, reference and named points, drive
/// points) rides the first page listing it; its channels follow, spilling
/// onto further pages that list the object by name only; its session
/// <c>values</c>, when present, start on a page of their own so a value
/// change re-sends only that page. A set that fits one datagram is a single
/// unpaged frame (<c>pageCount</c> 0).
/// </summary>
public static class DiscoveryPager
{
    /// <param name="newPage">Returns a fresh frame carrying the header and
    /// environment every page repeats; its object list is replaced.</param>
    /// <param name="objects">The complete descriptor set.</param>
    public static List<DiscoveryFrame> Paginate(Func<DiscoveryFrame> newPage,
                                                IReadOnlyList<ObjectDescriptor> objects,
                                                uint generation,
                                                int budget = DatagramBudget.Bytes)
    {
        var pages = new List<DiscoveryFrame>();
        DiscoveryFrame page = Open(newPage, pages);

        foreach (var source in objects)
        {
            var entry = Identity(source);
            page.Objects!.Add(entry);
            if (!Fits(page, budget))
            {
                page.Objects.RemoveAt(page.Objects.Count - 1);
                if (page.Objects.Count == 0)
                    throw new InvalidOperationException(
                        $"object '{source.Name}' identity alone exceeds the {budget}-byte datagram budget");
                page = Open(newPage, pages);
                page.Objects!.Add(entry);
                if (!Fits(page, budget))
                    throw new InvalidOperationException(
                        $"object '{source.Name}' identity alone exceeds the {budget}-byte datagram budget");
            }

            foreach (var channel in source.Channels ?? new List<ChannelDescriptor>())
                entry = Add(newPage, pages, ref page, entry, source.Name, budget,
                            e => e.Channels!, channel, $"channel {channel.Id} of '{source.Name}'");

            var values = source.Values;
            if (values is { Count: > 0 })
            {
                // session values ride their own page(s): open one unless the
                // current page holds nothing but this object's bare name entry
                bool bare = page.Objects!.Count == 1 && page.Objects[0] == entry &&
                            entry.Channels!.Count == 0 && IsContinuation(entry);
                if (!bare)
                {
                    page = Open(newPage, pages);
                    entry = Continuation(source.Name);
                    page.Objects!.Add(entry);
                }
                foreach (var value in values)
                    entry = Add(newPage, pages, ref page, entry, source.Name, budget,
                                e => e.Values!, value, $"value {value.Id} of '{source.Name}'");
            }
        }

        bool paged = pages.Count > 1;
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].Generation = generation;
            pages[i].Page = paged ? (ushort)i : (ushort)0;
            pages[i].PageCount = paged ? (ushort)pages.Count : (ushort)0;
            TrimEmptyLists(pages[i]);
        }
        return pages;
    }

    static ObjectDescriptor Add<T>(Func<DiscoveryFrame> newPage, List<DiscoveryFrame> pages,
                                   ref DiscoveryFrame page, ObjectDescriptor entry, string objectName,
                                   int budget, Func<ObjectDescriptor, List<T>> list, T item, string what)
    {
        list(entry).Add(item);
        if (Fits(page, budget)) return entry;
        list(entry).RemoveAt(list(entry).Count - 1);
        page = Open(newPage, pages);
        entry = Continuation(objectName);
        page.Objects!.Add(entry);
        list(entry).Add(item);
        if (!Fits(page, budget))
            throw new InvalidOperationException($"{what} alone exceeds the {budget}-byte datagram budget");
        return entry;
    }

    static DiscoveryFrame Open(Func<DiscoveryFrame> newPage, List<DiscoveryFrame> pages)
    {
        var page = newPage();
        page.Objects = new List<ObjectDescriptor>();
        // page fields are part of the measured size: measure as paged
        page.Page = (ushort)pages.Count;
        page.PageCount = ushort.MaxValue;
        pages.Add(page);
        return page;
    }

    static bool Fits(DiscoveryFrame page, int budget) =>
        DatagramBudget.SizeOf(FrameType.Discovery, page) <= budget;

    static ObjectDescriptor Identity(ObjectDescriptor source)
    {
        var entry = new ObjectDescriptor
        {
            Name = source.Name,
            Type = source.Type,
            Location = source.Location,
            TypeLabel = source.TypeLabel,
            Primary = source.Primary,
            NamedPoints = source.NamedPoints,
            DrivePoints = source.DrivePoints,
            Channels = new List<ChannelDescriptor>(),
            Values = new List<ChannelValue>(),
        };
        entry.ReferencePoint.Name = source.ReferencePoint.Name;
        entry.ReferencePoint.Position.X = source.ReferencePoint.Position.X;
        entry.ReferencePoint.Position.Y = source.ReferencePoint.Position.Y;
        entry.ReferencePoint.Position.Z = source.ReferencePoint.Position.Z;
        return entry;
    }

    static ObjectDescriptor Continuation(string name) => new ObjectDescriptor
    {
        Name = name,
        Channels = new List<ChannelDescriptor>(),
        Values = new List<ChannelValue>(),
    };

    static bool IsContinuation(ObjectDescriptor entry) =>
        entry.Type == default && !entry.Primary && entry.TypeLabel.Length == 0 &&
        entry.Location.Length == 0 && entry.DrivePoints is null && entry.NamedPoints is null;

    static void TrimEmptyLists(DiscoveryFrame page)
    {
        foreach (var o in page.Objects!)
        {
            if (o.Channels is { Count: 0 }) o.Channels = null;
            if (o.Values is { Count: 0 }) o.Values = null;
        }
    }
}

/// <summary>
/// Plans how a tick's channel values split into motion parts that each fit
/// the datagram budget (implementation-udp.md § Motion parts). Part 0
/// carries the object's pose, dynamics, local gravity and drive points plus
/// as many values as fit; later parts list the object by name with values
/// only. Sizes are measured once with the canonical encoder, using the
/// largest variants a frame can select, so the plan holds for every tick.
/// </summary>
public static class MotionPartPlanner
{
    /// <summary>Bytes one number or boolean ChannelValue adds to a list
    /// (one data word and one pointer word).</summary>
    public const int BytesPerValue = 16;

    /// <summary>Value ranges per part, in order: (start, count).</summary>
    /// <param name="part0">Part 0 as it will be sent, with exactly one
    /// channel value and the largest dynamics/gravity variants selected.</param>
    /// <param name="laterPart">A later part as it will be sent, with exactly
    /// one channel value. Both frames are measurement scratch: their part
    /// and generation fields are overwritten.</param>
    public static List<(int Start, int Count)> Plan(int valueCount, MotionFrame part0, MotionFrame laterPart,
                                                     int budget = DatagramBudget.Bytes)
    {
        var plan = new List<(int, int)>();
        if (valueCount == 0) { plan.Add((0, 0)); return plan; }
        int first = Capacity(part0, budget);
        int later = Capacity(laterPart, budget);
        int start = Math.Min(first, valueCount);
        plan.Add((0, start));
        while (start < valueCount)
        {
            int count = Math.Min(later, valueCount - start);
            plan.Add((start, count));
            start += count;
        }
        return plan;
    }

    static int Capacity(MotionFrame withOneValue, int budget)
    {
        // measured as a multi-part frame (part fields included)
        withOneValue.Part = 1;
        withOneValue.PartCount = ushort.MaxValue;
        withOneValue.Generation = uint.MaxValue;
        int one = DatagramBudget.SizeOf(FrameType.Motion, withOneValue);
        if (one > budget)
            throw new InvalidOperationException(
                $"a motion part with one channel value is {one} bytes, over the {budget}-byte datagram budget");
        return 1 + (budget - one) / BytesPerValue;
    }
}
}
