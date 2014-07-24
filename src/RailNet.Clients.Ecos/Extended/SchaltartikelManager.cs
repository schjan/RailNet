using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;
using RailNet.Core;
using RailNet.Core.Extended;
using TinyIoC;

namespace RailNet.Clients.Ecos.Extended
{
    public class SchaltartikelManager : ISchaltartikelManager
    {
        private readonly TinyIoCContainer _container;
        private readonly IBasicClient _basicClient;

        public SchaltartikelManager(TinyIoCContainer container)
        {
            _container = container;
            _basicClient = container.Resolve<IBasicClient>();
        }

        /// <summary>
        /// Setzt eine Digitaladresse direkt.
        /// </summary>
        /// <param name="adresse">Adresse</param>
        /// <param name="gruen">Grün = True; Rot = False</param>
        /// <param name="system">Verwendetes Digitalsystem</param>
        /// <returns></returns>
        public Task SetzeAdresse(int adresse, bool gruen, Digitalsystem system = Digitalsystem.DCC)
        {
            return _basicClient.Set(11, "switch", system.ToEcoSString() + adresse + (gruen ? "g" : "r"));
        }
    }
}
