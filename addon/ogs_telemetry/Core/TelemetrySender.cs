#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Ogs.Telemetry
{

/// <summary>
/// OGS-TS UDP-multicast sender. Default endpoint: 239.255.17.17:40123, TTL 1
/// (same subnet), loopback enabled for local development (see
/// implementation-udp.md).
/// </summary>
public sealed class TelemetrySender : IDisposable
{
    readonly Socket _socket;
    readonly IPEndPoint _endpoint;

    // Reused for every frame so a send allocates nothing in the encoder:
    // the words of the message, and the datagram built from them. Guarded
    // by _sync, since a socket can be shared between threads and these
    // cannot.
    readonly object _sync = new();
    readonly List<ulong> _words = new(256);
    byte[] _buffer = new byte[1500];

    /// <summary>The standard's IPv6 group (implementation-udp.md § Default
    /// endpoint): link-local, for networks without IPv4. A sender uses one
    /// address family per session.</summary>
    public const string DefaultGroupV6 = "ff12::4f47:5453";

    /// <param name="group">IPv4 or IPv6 multicast group (or a unicast
    /// address); the address family follows it.</param>
    /// <param name="localInterface">Egress NIC for multicast: for IPv4 that
    /// interface's local address, for IPv6 an address whose <c>ScopeId</c>
    /// is its index. Left null, the sender names the default route's
    /// interface rather than leaving the choice to the OS, whose choice for
    /// a group moves to loopback once anything on this host has sent to that
    /// group over loopback, and the stream then stops leaving the machine
    /// (implementation-udp.md § Network interfaces). On a multi-homed host
    /// (common on Linux servers, VM hosts, anything with a VPN adapter) the
    /// default route may have no path to the LAN: pass the interface you
    /// want when the host has more than one.</param>
    public TelemetrySender(string group = "239.255.17.17", int port = 40123,
                     int ttl = 1, bool loopback = true, IPAddress? localInterface = null)
    {
        var address = IPAddress.Parse(group);
        bool v6 = address.AddressFamily == AddressFamily.InterNetworkV6;
        var level = v6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP;
        _socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        _socket.SetSocketOption(level, SocketOptionName.MulticastTimeToLive, ttl); // hop limit on IPv6
        if (loopback)
            _socket.SetSocketOption(level, SocketOptionName.MulticastLoopback, true);
        if (localInterface is not null)
        {
            if (v6) _socket.SetSocketOption(level, SocketOptionName.MulticastInterface, (int)localInterface.ScopeId);
            else _socket.SetSocketOption(level, SocketOptionName.MulticastInterface, localInterface.GetAddressBytes());
        }
        else if (IsMulticast(address))
        {
            SendOnDefaultRoute(v6, level);
        }
        _endpoint = new IPEndPoint(address, port);
    }

    static bool IsMulticast(IPAddress address)
    {
        if (address.AddressFamily == AddressFamily.InterNetworkV6) return address.IsIPv6Multicast;
        byte first = address.GetAddressBytes()[0];
        return first >= 224 && first <= 239;
    }

    /// <summary>Best-effort: with no default route, or if the interface
    /// refuses, the OS chooses.</summary>
    void SendOnDefaultRoute(bool v6, SocketOptionLevel level)
    {
        try
        {
            if (v6)
            {
                if (DefaultInterfaceIndexV6() is int index)
                    _socket.SetSocketOption(level, SocketOptionName.MulticastInterface, index);
            }
            else if (DefaultRouteAddress() is IPAddress nic)
            {
                _socket.SetSocketOption(level, SocketOptionName.MulticastInterface, nic.GetAddressBytes());
            }
        }
        catch (SocketException)
        {
        }
    }

    /// <summary>Encodes <paramref name="frame"/> in canonical OGS encoding
    /// and sends it as one datagram. The encoding reuses this sender's
    /// buffers; the bytes sent are those of
    /// <see cref="Wire.CanonicalEncoder.EncodeDatagram"/>.</summary>
    public void Send(FrameType type, Wire.WireStruct frame)
    {
        lock (_sync)
        {
            Wire.CanonicalEncoder.EncodeInto(_words, frame);
            int length = Envelope.EncodeDatagram(ref _buffer, type, _words);
            SendBuffer(_buffer, length);
        }
    }

    /// <summary>Sends a complete OMT1 datagram as-is. Telemetry is lossy by
    /// design, so a transient socket failure (interface down, no route) is
    /// counted in <see cref="SendFailures"/> rather than thrown: it must not
    /// abort the publisher part-way through a tick.</summary>
    public void Send(byte[] datagram) => SendBuffer(datagram, datagram.Length);

    void SendBuffer(byte[] buffer, int length)
    {
        try { _socket.SendTo(buffer, 0, length, SocketFlags.None, _endpoint); }
        catch (SocketException ex)
        {
            LastSendError = ex;
            SendFailures++;
        }
    }

    /// <summary>Datagrams that failed to send with a socket error.</summary>
    public long SendFailures { get; private set; }

    /// <summary>The most recent send's socket error, or null.</summary>
    public SocketException? LastSendError { get; private set; }

    public void Dispose() => _socket.Dispose();

    /// <summary>This host's IPv4 address on its default route, or null when
    /// it has none. Connecting a UDP socket only looks up the route, and that
    /// of a unicast address, which multicast traffic can't move: nothing is
    /// sent. 192.0.2.1 is reserved for documentation (RFC 5737).</summary>
    public static IPAddress? DefaultRouteAddress()
    {
        try
        {
            using var probe = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            probe.Connect(new IPEndPoint(IPAddress.Parse("192.0.2.1"), 9));
            var address = ((IPEndPoint)probe.LocalEndPoint!).Address;
            return address.Equals(IPAddress.Any) || IPAddress.IsLoopback(address) ? null : address;
        }
        catch (SocketException)
        {
            return null;
        }
    }

    /// <summary>The interface to use the link-local IPv6 group on, or null
    /// when none is found: the IPv4 default route's interface if it has IPv6,
    /// otherwise the first with an IPv6 gateway, otherwise the first with
    /// IPv6. A link-local group has no route to look up.</summary>
    public static int? DefaultInterfaceIndexV6()
    {
        try
        {
            var v4 = DefaultRouteAddress();
            int? withGateway = null, first = null;
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up || !nic.SupportsMulticast
                    || !nic.Supports(NetworkInterfaceComponent.IPv6)
                    || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback
                    || nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;
                var properties = nic.GetIPProperties();
                int index = properties.GetIPv6Properties().Index;
                first ??= index;
                foreach (var gateway in properties.GatewayAddresses)
                    if (gateway.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        withGateway ??= index;
                if (v4 is null) continue;
                foreach (var unicast in properties.UnicastAddresses)
                    if (unicast.Address.Equals(v4)) return index;
            }
            return withGateway ?? first;
        }
        catch (Exception)
        {
            return null; // not every platform can list its interfaces
        }
    }
}
}
