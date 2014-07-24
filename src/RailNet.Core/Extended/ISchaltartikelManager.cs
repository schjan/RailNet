using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Core.Extended
{
    public interface ISchaltartikelManager
    {
        /// <summary>
        /// Setzt eine Digitaladresse direkt.
        /// </summary>
        /// <param name="adresse">Adresse</param>
        /// <param name="gruen">Grün = True; Rot = False</param>
        /// <param name="system">Verwendetes Digitalsystem</param>
        /// <returns></returns>
        Task SetzeAdresse(int adresse, bool gruen, Digitalsystem system = Digitalsystem.DCC);
    }
}
