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
using NLog;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Extended;
using RailNet.Clients.Ecos.Network;
using RailNet.Core;
using RailNet.Core.Extended;

namespace RailNet.Clients.Ecos
{
    /// <summary>
    /// Hauptklasse zur Verbindung mit der Modellbahnanlage
    /// <see cref="RailNetClientBase"/>
    /// </summary>
    public class RailClient : RailNetClientBase, INotifyPropertyChanged
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private INetworkClient _networkClient;
        protected INetworkClient NetworkClient
        {
            get { return _networkClient; }
        }

        private INachrichtenDispo _nachrichtenDispo;
        protected INachrichtenDispo NachrichtenDispo
        {
            get { return _nachrichtenDispo; }
        }

        private ISchaltartikelManager _schaltartikel;
        public override ISchaltartikelManager Schaltartikel
        {
            get { return _schaltartikel; }
            protected set { _schaltartikel = value; }
        }

        /// <summary>
        /// Gibt an, ob der Client mit der ECoS verbunden ist.
        /// </summary>
        public override bool Connected
        {
            get
            {
                if (NetworkClient != null)
                    return NetworkClient.Connected;
                return false;
            }
        }

        private IBasicClient _basicClient;
        /// <summary>
        /// <see cref="IBasicClient"/>
        /// </summary>
        public IBasicClient BasicClient
        {
            get { return _basicClient; }
        }

        /// <summary>
        /// Verbindet sich asynchron mit der ECoS und setzt den Status initial.
        /// </summary>
        /// <param name="host">Hostname</param>
        /// <param name="port">Standartmäßig 15471</param>
        /// <returns>Erfolg</returns>
        public async Task<bool> ConnectAsync(string host, int port = 15471)
        {
            var result = await NetworkClient.ConnectAsync(host, port);

            if (result == SocketError.Success)
            {
                NachrichtenDispo.IncomingEvents.Where(x => x.Receiver == 1).Subscribe(BasisobjektEventsAuswerten);

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

            logger.Trace(() => ("Status changed to " + _status));
        }

        /// <summary>
        /// Beendet die Verbindung mit der ECoS
        /// </summary>
        public override void Disconnect()
        {
            NetworkClient.Disconnect();
        }

        public RailClient()
        {
            SetUpIoC();
            SetUpComponents();
        }

        private void SetUpIoC()
        {
            _networkClient = new NetworkClient();
            _nachrichtenDispo = new NachrichtenDispo(_networkClient);
            _basicClient = new BasicClient(NachrichtenDispo);
        }

        private void SetUpComponents()
        {
            Schaltartikel = new SchaltartikelManager(BasicClient);
        }

        private RailStatus _status;

        /// <summary>
        /// Gibt den Anlagenstatus aus oder setzt ihn.
        /// </summary>
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
