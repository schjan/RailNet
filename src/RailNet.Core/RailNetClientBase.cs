using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailNet.Core.Extended;

namespace RailNet.Core
{
    public abstract class RailNetClientBase : IRailNetClient
    {
        protected RailNetClientBase()
        {
            
        }

        public abstract ISchaltartikelManager Schaltartikel { get; protected set; }

        public abstract bool Connected { get; }
        public abstract void Disconnect();
    }
}
