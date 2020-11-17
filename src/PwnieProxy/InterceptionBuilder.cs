using System.IO;
using PwnieProxy.Handlers;

namespace PwnieProxy
{
    public static class InterceptionBuilder
    {
        public static HandlerBuilder AddStreams(Stream toServer, Stream toClient, Stream? copyToServer = null, Stream? copyToClient = null)
        {
            return new HandlerBuilder(toServer, toClient, copyToServer, copyToClient);
        }

        public class HandlerBuilder
        {
            private readonly InterceptionStream toServer;
            private readonly InterceptionStream toClient;

            internal HandlerBuilder(Stream toServer, Stream toClient, Stream? copyToServer = null, Stream? copyToClient = null)
            {
                this.toServer = new InterceptionStream(toServer, copyToServer);
                this.toClient = new InterceptionStream(toClient, copyToClient);
            }

            public HandlerBuilder AddToServerHandler(IHandler handler)
            {
                handler.Other = toClient;
                toServer.AddHandler(handler);
                return this;
            }


            public HandlerBuilder AddToClientHandler(IHandler handler)
            {
                handler.Other = toServer;
                toClient.AddHandler(handler);
                return this;
            }

            public (InterceptionStream toServer, InterceptionStream toClient) Build()
            {
                return (toServer, toClient);
            }
        }
    }
}