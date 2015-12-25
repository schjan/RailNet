using System;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos.Basic
{
    internal interface INachrichtenDispo
    {
        /// <summary>
        /// Von ausserhalb Aufgerufene Methode zum senden einer Nachricht.
        /// Sendet eine Nachricht und erwartet die Antwort zu der Nachricht.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<BasicResponse> SendCommandAsync(string command);

        /// <summary>
        /// Eine IObservable von Serverseitigen Events
        /// </summary>
        IObservable<BasicEvent> IncomingEvents { get; }
    }
}
