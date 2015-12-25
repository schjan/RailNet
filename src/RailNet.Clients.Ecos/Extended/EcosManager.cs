using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;

namespace RailNet.Clients.Ecos.Extended
{
    public class EcosManager
    {
        private readonly IBasicClient _basicClient;
        private bool _subscribesToStatus = false;

        public string HardwareVersion = string.Empty;
        public string ApplicationVersion = string.Empty;
        public string ProtocolVersion = string.Empty;

        public EcosManager(IBasicClient basicClient)
        {
            _basicClient = basicClient;

            StatusObservable =
                _basicClient.EventObservable.Where(x => x.Receiver == 1 && (x.Content[0].Contains("status")))
                    .Select(m =>
                    {
                        var status = BasicParser.TryGetParameterFromContent("status", m.Content[0]);
                        return status == "GO";
                    });
        }

        ~EcosManager()
        {
            if (_subscribesToStatus)
                UnsubscribeFromStatus();
        }

        public Task Go()
        {
            return _basicClient.Set(StaticIds.EcosId, "go");
        }

        public Task Stop()
        {
            return _basicClient.Set(StaticIds.EcosId, "stop");
        }

        public Task SubscribeToStatus()
        {
            _subscribesToStatus = true;
            return _basicClient.Request(1, "view");
        }

        public Task UnsubscribeFromStatus()
        {
            _subscribesToStatus = false;
            return _basicClient.Release(1, "view");
        }

        public async Task<bool> GetStatus()
        {
            var result = await _basicClient.Get(StaticIds.EcosId, "status");

            return result.Content[0].Contains("GO");
        }

        public async Task<bool> UpdateInfo()
        {
            var result = await _basicClient.Get(StaticIds.EcosId, "info");
            if (result.HasError)
                return false;

            ProtocolVersion = BasicParser.TryGetParameterFromContent("ProtocolVersion", result.Content[1]);
            ApplicationVersion = BasicParser.TryGetParameterFromContent("ApplicationVersion", result.Content[2]);
            HardwareVersion = BasicParser.TryGetParameterFromContent("HardwareVersion", result.Content[3]);

            return true;
        }

        public IObservable<bool> StatusObservable { get; }
    }
}