using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos.Basic
{
    public interface INachrichtenDispo
    {
        /// <summary>
        /// Von ausserhalb Aufgerufene Methode zum senden einer Nachricht.
        /// Sendet eine Nachricht und erwartet die Antwort zu der Nachricht.
        /// </summary>
        /// <param name="befehl"></param>
        /// <returns></returns>
        Task<BasicAntwort> SendeBefehlAsync(string befehl);
    }
}
