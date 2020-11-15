using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PwnieProxy
{
    public class TcpProxy
    {
        public async Task Start(string remoteServerIp, ushort remoteServerPort, ushort localPort, string? localIp = null)
        {
            var localIpAddress = string.IsNullOrEmpty(localIp) ? IPAddress.Loopback : IPAddress.Parse(localIp);
            var server = new TcpListener(new IPEndPoint(localIpAddress, localPort));
            server.Start();
            Console.WriteLine($"TCP proxy started {localPort} -> {remoteServerIp}|{remoteServerPort}");
            while (true)
            {
                try
                {
                    var remoteClient = await server.AcceptTcpClientAsync();
                    remoteClient.NoDelay = true;
                    var ips = await Dns.GetHostAddressesAsync(remoteServerIp);
                    var client = new TcpClient(remoteClient, new IPEndPoint(ips.First(), remoteServerPort));
                }
                catch (Exception ex) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex);
                    Console.ResetColor();
                }

            }
        }
    }
}