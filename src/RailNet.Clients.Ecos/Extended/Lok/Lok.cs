using System.Threading.Tasks;
using PropertyChanged;
using RailNet.Clients.Ecos.Basic;

namespace RailNet.Clients.Ecos.Extended.Lok
{
    [ImplementPropertyChanged]
    public class Lok
    {
        private readonly LokManager _lokManager;
        private readonly IBasicClient _basicClient;

        internal Lok(int id, LokManager lokManager, IBasicClient basicClient, byte maxFahrstufe)
        {
            _lokManager = lokManager;
            _basicClient = basicClient;
            MaxFahrstufe = maxFahrstufe;
            Id = id;
        }

        public int Id { get; }

        /// <summary>
        /// Anzeigename der Lok innerhalb der ECoS
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Aktuelle Geschwindigkeitsstufe
        /// </summary>
        public byte Fahrstufe { get; internal set; }

        /// <summary>
        /// Anzahl der verfügbaren Geschwindigkeitsstufen.
        /// </summary>
        public byte MaxFahrstufe { get; }

        /// <summary>
        /// Aktuelle Fahrtrichtung des Fahrzeuges
        /// </summary>
        public Fahrtrichtung Fahrtrichtung { get; set; } = Fahrtrichtung.Undefined;

        public bool HasControl { get; internal set; }

        public bool HasView { get; internal set; }

        public async Task<bool> GetView()
        {
            var result = await _basicClient.Request(Id, "view");

            return HasView = result.HasError;
        }

        public Task ReleaseView()
        {
            HasView = false;
            return _basicClient.Release(Id, "view");
        }

        public async Task<bool> GetControl(bool force = false)
        {
            var result = await _basicClient.Request(Id, "control", force);
            if (result.HasError)
                return HasControl = false;
            return HasControl = true;
        }

        public Task ReleaseControl()
        {
            HasControl = false;
            return _basicClient.Release(Id, "control");
        }

        public async Task<bool> SetFahrstufe(byte fahrstufe)
        {
            if (!HasControl)
                return false;

            if (Fahrstufe == fahrstufe)
                return true;

            if (fahrstufe > MaxFahrstufe)
                fahrstufe = MaxFahrstufe;

            var result = await _basicClient.Set(Id, "speedstep", fahrstufe.ToString());

            return result.HasError;
        }

        public async Task<bool> SetFahrtrichtung(bool vorwaerts)
        {
            // Rückwärts = 1, Vorwärts = 0!
            var result = await _basicClient.Set(Id, "dir", vorwaerts ? "0" : "1");
            
            return result.HasError;
        }

        public Task<bool> SetFahrtrichtung(Fahrtrichtung fahrtrichtung)
        {
            if (fahrtrichtung == Fahrtrichtung.Undefined)
                return Task.FromResult(false);

            return SetFahrtrichtung(fahrtrichtung == Fahrtrichtung.Vorwaerts);
        }
    }
}
