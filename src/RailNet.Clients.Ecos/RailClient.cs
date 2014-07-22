using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Network;
using RailNet.Core;
using TinyIoC;

namespace RailNet.Clients.Ecos
{
    public class RailClient : RailNetClientBase, INotifyPropertyChanged
    {
        private readonly TinyIoCContainer container;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private INetworkClient networkClient
        {
            get { return container.Resolve<INetworkClient>(); }
        }

        private INachrichtenDispo nachrichtenDispo
        {
            get { return container.Resolve<INachrichtenDispo>(); }
        }

        public override bool Connected
        {
            get
            {
                if (container.CanResolve<INetworkClient>())
                    return networkClient.Connected;
                return false;
            }
        }

        public IBasicClient BasicClient
        {
            get { return container.Resolve<IBasicClient>(); }
        }

        public async Task<bool> ConnectAsync(string host, int port = 15471)
        {
            var result = await networkClient.ConnectAsync(host, port);

            if (result == SocketError.Success)
            {
                nachrichtenDispo.IncomingEvents.Where(x => x.Receiver == 1).Subscribe(BasisobjektEventsAuswerten);

                Task t1 = BasicClient.Request(1, "view");
                Task t2 = SetInitialStatus();
                await Task.WhenAll(t1, t2);


                return true;
            }

            return false;
        }

        private void BasisobjektEventsAuswerten(BasicEvent e)
        {
            var res = BasicParser.ParseContent(e.Content).ToArray();
            SetStatusByContent(res[0]);
        }

        private async Task SetInitialStatus()
        {
            var res = await BasicClient.Get(1, "status");
            SetStatusByContent(BasicParser.ParseContent(res.Content).ToArray()[0]);
        }

        private void SetStatusByContent(string[] content)
        {
            if (content[1] != "status")
                return;

            switch (content[2])
            {
                case "GO":
                    _status = RailStatus.Go;
                    break;
                case "STOP":
                    _status = RailStatus.Stop;
                    break;
            }
            OnPropertyChanged("Status");

            logger.Trace("Status changed to {0}", _status.ToString());
        }

        public override void Disconnect()
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

        private RailStatus _status;

        public RailStatus Status
        {
            get { return _status; }
            set
            {
                if (Connected)
                {
                    switch (value)
                    {
                        case RailStatus.Go:
                            BasicClient.Set(1, "go");
                            break;
                        case RailStatus.Stop:
                            BasicClient.Set(1, "stop");
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
