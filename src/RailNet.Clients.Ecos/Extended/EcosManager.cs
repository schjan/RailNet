using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;

namespace RailNet.Clients.Ecos.Extended
{
    public class EcosManager
    {
        private readonly IBasicClient _basicClient;

        public string HardwareVersion = string.Empty;
        public string ApplicationVersion = string.Empty;
        public string ProtocolVersion = string.Empty;
        
        public EcosManager(IBasicClient basicClient)
        {
            _basicClient = basicClient;
        }

        public Task Go()
        {
            return _basicClient.Set(StaticIds.EcosId, "go");
        }

        public Task Stop()
        {
            return _basicClient.Set(StaticIds.EcosId, "stop");
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
    }
}