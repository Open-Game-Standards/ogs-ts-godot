#nullable enable
using System;
using System.Net;
using System.Net.Sockets;

namespace Ogsa.Telemetry
{

/// <summary>
/// LTS UDP-multicast sender. Default endpoint: 239.255.17.17:40123, TTL 1
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
    public TelemetrySender(string group = "239.255.17.17", int port = 40123,
                     int ttl = 1, bool loopback = true, IPAddress? localInterface = null)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
        if (loopback)
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
        if (localInterface is not null)
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface,
                localInterface.GetAddressBytes());
        _endpoint = new IPEndPoint(IPAddress.Parse(group), port);
    }

    public void Send(FrameType type, Capnp.MessageBuilder message) =>
        _socket.SendTo(Envelope.EncodeDatagram(type, message.Frame.Segments), _endpoint);

    public void Dispose() => _socket.Dispose();
}
}
