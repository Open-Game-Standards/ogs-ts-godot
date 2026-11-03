#nullable enable
using System;
using System.Net;
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

    /// <param name="localInterface">Egress NIC for the multicast datagrams,
    /// as that interface's local IPv4 address. Left null, the OS default
    /// route picks the interface — on a multi-homed host (common on Linux
    /// servers, VM hosts, anything with a VPN adapter) that can silently
    /// pick a NIC with no path to the LAN. Pass the interface you want when
    /// the host has more than one.</param>
    /// <summary>The standard's IPv6 group (implementation-udp.md § Default
    /// endpoint): link-local, for networks without IPv4. A sender uses one
    /// address family per session.</summary>
    public const string DefaultGroupV6 = "ff12::4f47:5453";

    /// <param name="group">IPv4 or IPv6 multicast group (or a unicast
    /// address); the address family follows it.</param>
    /// <param name="localInterface">For IPv6, an address whose
    /// <c>ScopeId</c> is the interface index to send on.</param>
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
        _endpoint = new IPEndPoint(address, port);
    }

    /// <summary>Encodes <paramref name="frame"/> in canonical OGS encoding
    /// and sends it as one datagram.</summary>
    public void Send(FrameType type, Wire.WireStruct frame) =>
        Send(Wire.CanonicalEncoder.EncodeDatagram(type, frame));

    /// <summary>Sends a complete OMT1 datagram as-is.</summary>
    public void Send(byte[] datagram) => _socket.SendTo(datagram, _endpoint);

    public void Dispose() => _socket.Dispose();
}
}
