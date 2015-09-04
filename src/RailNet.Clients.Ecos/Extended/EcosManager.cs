using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;

namespace RailNet.Clients.Ecos.Extended
{
    public class EcosManager
    {
        private readonly IBasicClient _basicClient;

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
    }
}