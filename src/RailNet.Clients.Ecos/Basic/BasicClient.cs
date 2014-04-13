using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos.Basic
{
    public class BasicClient : IBasicClient
    {
        public async Task<BasicAntwort> QueryObjects(int id, params string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
