using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Extended;
using RailNet.Clients.Ecos.Extended.Lok;
using RailNet.Clients.Ecos.Extended.Rueckmeldung;
using RailNet.Clients.Ecos.Network;
using RailNet.Core;
using RailNet.Core.Extended;
using RailNet.Core.Logging;

namespace RailNet.Clients.Ecos
{
    /// <summary>
    /// Hauptklasse zur Verbindung mit der Modellbahnanlage
    /// <see cref="RailNetClientBase"/>
    /// </summary>
    public class EcosClient : RailNetClientBase, INotifyPropertyChanged
    {
        protected INetworkClient NetworkClient { get; private set; }

        internal INachrichtenDispo NachrichtenDispo { get; private set; }

        /// <summary>
        /// <see cref="SchaltartikelManager"/>
        /// </summary>
        public override ISchaltartikelManager Schaltartikel { get; protected set; }

        /// <summary>
        /// Gibt an, ob der Client mit der ECoS verbunden ist.
        /// </summary>
        public override bool Connected => NetworkClient != null && NetworkClient.Connected;

        /// <summary>
        /// <see cref="IBasicClient"/>
        /// </summary>
        public IBasicClient BasicClient { get; private set; }

        /// <summary>
        /// <see cref="RueckmeldeManager"/>
        /// </summary>
        public RueckmeldeManager Rueckmelder { get; private set; }

        /// <summary>
        /// <see cref="LokManager"/>
        /// </summary>
        public LokManager Loks { get; private set; }

        /// <summary>
        /// <see cref="EcosManager"/>
        /// </summary>
        public EcosManager Ecos { get; private set; }

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

            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged("Status");

            Logger.Trace("Status changed to " + _status);
        }

        /// <summary>
        /// Beendet die Verbindung mit der ECoS
        /// </summary>
        public override void Disconnect()
        {
            NetworkClient.Disconnect();
        }

        public EcosClient(IRailLogger logger = null) : base(logger)
        {
            SetUpIoC();
            SetUpComponents();
        }

        private void SetUpIoC()
        {
            NetworkClient = new NetworkClient();
            NachrichtenDispo = new NachrichtenDispo(NetworkClient);
            BasicClient = new BasicClient(NachrichtenDispo);
        }

        private void SetUpComponents()
        {
            Schaltartikel = new SchaltartikelManager(BasicClient);
            Rueckmelder = new RueckmeldeManager(BasicClient);
            Schaltartikel = new SchaltartikelManager(BasicClient);
            Ecos = new EcosManager(BasicClient);
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
