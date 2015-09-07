using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;

namespace RailNet.Clients.Ecos.Extended.Lok
{
    public class LokManager
    {
        private readonly IBasicClient _basicClient;
        internal readonly Dictionary<int, Lok> Loks;
        private IDisposable _subscription;

        public LokManager(IBasicClient basicClient)
        {
            _basicClient = basicClient;
            Loks = new Dictionary<int, Lok>();

            _subscription =
                _basicClient.EventObservable.Where(x => x.Receiver == 10 || (1000 <= x.Receiver && x.Receiver < 1999))
                    .Subscribe(HandleEvent);
        }

        public async Task<bool> QueryAll()
        {
            var result = await _basicClient.QueryObjects(10, "name", "addr", "protocol");

            if (result.HasError)
                return false;

            foreach (var lokObject in result.Content)
            {
                var id = Convert.ToInt32(lokObject.Split(' ').First());
                var name = BasicParser.TryGetParameterFromContent("name", lokObject, true);
                var protocol = BasicParser.TryGetParameterFromContent("protocol", lokObject);
                var adress = BasicParser.TryGetParameterFromContent("addr", lokObject);

                if (Loks.ContainsKey(id))
                    continue;

                Loks.Add(id, new Lok(id, this, _basicClient, GetFahrstufenByProtocol(protocol)) {Name = name});
            }

            return true;
        }

        public async Task<bool> RequestChanges()
        {
            var result = await _basicClient.Request(10, BefehlStrings.ViewS);
            return result.HasError;
        }

        private static byte GetFahrstufenByProtocol(string protocol)
        {
            switch (protocol)
            {
                case "MFX":
                    return 127;
                case "MM28":
                    return 28;
                case "MM27":
                    return 27;
                case "MM14":
                    return 14;
                case "DCC14":
                    return 14;
                case "DCC128":
                    return 128;
                case "DCC28":
                    return 28;
                case "MULTI":
                    return 126; //?
                default:
                    return 0;
            }
        }

        private void HandleEvent(BasicEvent evt)
        {
            if (evt.Receiver != 10)
            {
                HandleLokEvent(evt);
            }
        }

        private void HandleLokEvent(BasicEvent evt)
        {
            if (!Loks.ContainsKey(evt.Receiver))
                return;

            var lok = Loks[evt.Receiver];

            foreach (var s in evt.Content)
            {
                if (s.Contains("speedstep"))
                    lok.Fahrstufe = Convert.ToByte(BasicParser.TryGetParameterFromContent("speedstep", s));
                if (s.Contains("dir"))
                    lok.Fahrtrichtung = BasicParser.TryGetParameterFromContent("dir", s) == "0"
                        ? Fahrtrichtung.Vorwaerts
                        : Fahrtrichtung.Rueckwaerts;
            }
        }
    }
}