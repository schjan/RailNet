using RailNet.Core.Extended;
using RailNet.Core.Logging;

namespace RailNet.Core
{
    public abstract class RailNetClientBase : IRailNetClient
    {
        internal static IRailLogger Logger;

        protected RailNetClientBase(IRailLogger logger = null)
        {
            Logger = logger ?? new NullLogger();
        }

        public abstract ISchaltartikelManager Schaltartikel { get; protected set; }

        public abstract bool Connected { get; }

        public abstract void Disconnect();
    }
}
