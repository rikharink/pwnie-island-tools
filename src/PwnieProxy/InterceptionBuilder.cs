using System.IO;
using PwnieProxy.Handlers;

namespace PwnieProxy
{
    public class InterceptionBuilder
    {
        public static HandlerBuilder AddStream(Stream toServer, Stream toClient)
        {
            return new HandlerBuilder(toServer, toClient);
        }

        public class HandlerBuilder
        {
            private InterceptionStream toServer;
            private InterceptionStream toClient;
            
            internal HandlerBuilder(Stream toServer, Stream toClient)
            {
                this.toServer = new InterceptionStream(toServer);
                this.toClient = new InterceptionStream(toClient);
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