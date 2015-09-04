using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;
using RailNet.Clients.Ecos.Basic;
using static RailNet.Clients.Ecos.Basic.BefehlStrings;

namespace RailNet.Clients.Ecos.Extended
{
    public class RueckmeldeManager
    {
        private readonly IBasicClient _basicClient;
        private readonly ILogger _logger;
        private readonly IDisposable _eventHandler;
        internal readonly Dictionary<int, RueckmeldeModul> Module;

        public RueckmeldeManager(IBasicClient basicClient)
        {
            _logger = LogManager.GetCurrentClassLogger();

            Module = new Dictionary<int, RueckmeldeModul>();

            _basicClient = basicClient;

            _eventHandler =
                _basicClient.EventObservable.Where(x => IsRueckmeldemodulId(x.Receiver))
                    .Subscribe(HandleRueckmeldeEvent);
        }

        ~RueckmeldeManager()
        {
            _eventHandler?.Dispose();
        }

        private bool IsRueckmeldemodulId(int id)
        {
            return (100 <= id && id <= 163)
                   || (200 <= id && id <= 280);
        }

        private void HandleRueckmeldeEvent(BasicEvent evt)
        {
            if(!Module.ContainsKey(evt.Receiver))
                return;

            var result = BasicParser.TryGetParameterFromContent("state", evt.Content[0]);
            result = result.Substring(2);

            var belegung = Convert.ToInt16(result, 16);
            
            var modul = Module[evt.Receiver];

            for (var i = 0; i < modul.Ports; i++)
            {
                modul.Rueckmelder[i].Belegt = (belegung & (1 << i)) != 0;
            }
        }

        public async Task SubscribeAll()
        {
            var response = await _basicClient.QueryObjects(StaticIds.FeedbackManagerId);

            if (response.HasError)
                throw new InvalidDataReceivedException("Fehler beim Abrufen der Rückmeldemodule");

            foreach (var id in response.Content.Select(mod => Convert.ToInt32(mod)))
            {
                var getPortResponse = await _basicClient.Get(id, PortsS);
                var reqresponse = await _basicClient.Request(id, ViewS);

                if (reqresponse.HasError || getPortResponse.HasError)
                    _logger.Error($"Konnte nicht mit Rückmelder {id} verbinden");
                else
                {
                    var ports =
                        Convert.ToInt32(BasicParser.TryGetParameterFromContent("ports", getPortResponse.Content[0]));

                    Module.Add(id, new RueckmeldeModul(id, ports));
                }
            }
        }

        public async Task UnsubscribeAll()
        {
            var messages =
                Module.Select(rueckmeldeModul => _basicClient.Release(rueckmeldeModul.Key, ViewS)).Cast<Task>().ToList();

            await Task.WhenAll(messages);
            
            Module.Clear();
        }
    }
}
