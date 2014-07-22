using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Core
{
    public abstract class RailNetClientBase : IRailNetClient
    {
        protected RailNetClientBase()
        {
            
        }

        public abstract bool Connected { get; }
        public abstract void Disconnect();
    }
}
