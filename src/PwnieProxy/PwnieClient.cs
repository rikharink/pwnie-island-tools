using System;
using System.Net;
using System.Threading.Tasks;
using PwnieProxy.Handlers;

namespace PwnieProxy
{
    public class PwnieClient
    {
        private System.Net.Sockets.TcpClient _remoteClient;
        private readonly IPEndPoint? _clientEndpoint;
        private readonly IPEndPoint _remoteServer;
        private readonly System.Net.Sockets.TcpClient _client = new System.Net.Sockets.TcpClient();

        public PwnieClient(System.Net.Sockets.TcpClient remoteClient, IPEndPoint remoteServer)
        {
            _remoteClient = remoteClient;
            _remoteServer = remoteServer;
            _client.NoDelay = true;
            _clientEndpoint = (IPEndPoint?)_remoteClient.Client.RemoteEndPoint;
            Console.WriteLine($"Established {_clientEndpoint} => {remoteServer}");
            Run();
        }

        private void Run()
        {

            Task.Run(async () =>
            {
                try
                {
                    using (_remoteClient)
                    using (_client)
                    {
                        await _client.ConnectAsync(_remoteServer.Address, _remoteServer.Port);
                        (var toServer, var toClient) =
                            InterceptionBuilder
                                .AddStreams(_client.GetStream(), _remoteClient.GetStream())
                                .AddToServerHandler(new PositionLoggerHandler())
                                .AddToServerHandler(new PacketLogger("[Client -> Server]"))
                                .AddToClientHandler(new PacketLogger("[Server -> Client]"))
                                .AddToServerHandler(new ChatHandler())
                                .Build();

                        await Task.WhenAny(toClient.CopyToAsync(toServer),
                            toServer.CopyToAsync(toClient));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                finally
                {
                    Console.WriteLine($"Closed {_clientEndpoint} => {_remoteServer}");
                    _remoteClient = null!;
                }
            });
        }

    }
}