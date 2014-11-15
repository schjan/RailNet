using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;
using RailNet.Core;
using RailNet.Core.Extended;

namespace RailNet.Clients.Ecos.Extended
{
    public class SchaltartikelManager : ISchaltartikelManager
    {
        private readonly IBasicClient _basicClient;

        public SchaltartikelManager(IBasicClient basicClient)
        {
            _basicClient = basicClient;
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
