using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PwnieProxy
{
    public class TcpClient
    {
        private System.Net.Sockets.TcpClient _remoteClient;
        private readonly IPEndPoint _clientEndpoint;
        private readonly IPEndPoint _remoteServer;
        private readonly System.Net.Sockets.TcpClient _client = new System.Net.Sockets.TcpClient();

        public TcpClient(System.Net.Sockets.TcpClient remoteClient, IPEndPoint remoteServer)
        {
            _remoteClient = remoteClient;
            _remoteServer = remoteServer;
            _client.NoDelay = true;
            _clientEndpoint = (IPEndPoint) _remoteClient.Client.RemoteEndPoint;
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
                    await using (var serverLogStream = File.OpenWrite(@"server.dump"))
                    await using(var remoteLogStream = File.OpenWrite(@"remote.dump"))
                    {
                        await _client.ConnectAsync(_remoteServer.Address, _remoteServer.Port);
                        var serverStream = new InterceptionStream(_client.GetStream(), serverLogStream);
                        var remoteStream = new InterceptionStream(_remoteClient.GetStream(), remoteLogStream);
                        await Task.WhenAny(remoteStream.CopyToAsync(serverStream),
                            serverStream.CopyToAsync(remoteStream));
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