using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Network;
using RailNet.Core;
using TinyIoC;

namespace RailNet.Clients.Ecos
{
    public class RailClient : RailNetClientBase
    {
        private readonly TinyIoCContainer container;

        private INetworkClient networkClient
        {
            get { return container.Resolve<INetworkClient>(); }
        }

        public bool Connected
        {
            get
            {
                if (container.CanResolve<INetworkClient>())
                    return networkClient.Connected;
                return false;
            }
        }

        public IBasicClient BasicClient {get { return container.Resolve<IBasicClient>(); }}

        public async Task<bool> ConnectAsync(string host, int port = 15471)
        {
            var result = await networkClient.ConnectAsync(host, port);

            if (result == SocketError.Success)
                return true;
            return false;
        }

        public void Disconnect()
        {
            networkClient.Disconnect();
        }

        public RailClient()
        {
            container = TinyIoCContainer.Current;

            SetUpIoC();
        }

        private void SetUpIoC()
        {
            container.Register<INetworkClient, NetworkClient>();
            container.Register<INachrichtenDispo, NachrichtenDispo>();
            container.Register<IBasicClient, BasicClient>();
            container.Register<IRailNetClient, RailClient>();
        }
    }
}
