using System.Collections.Generic;

namespace RailNet.Clients.Ecos.Basic
{
    /// <summary>
    /// Event wie es von der ECOS an den Client gesendet wird.
    /// </summary>
    public class BasicEvent : BasicMessage
    {
        public int Receiver;
        
        /// <summary>
        /// Erstellt eine neue Instanz der <see cref="BasicEvent"/> Klasse.
        /// </summary>
        /// <param name="errorNumber">Fehlernummer</param>
        /// <param name="error">Fehlerbeschreibung</param>
        public BasicEvent(int errorNumber, string error)
            : this(null, errorNumber, error)
        {
        }

        /// <summary>
        /// Erstellt eine neue Instanz der <see cref="BasicEvent"/> Klasse.
        /// </summary>
        /// <param name="message">Einzelne Zeilen der Nachricht</param>
        public BasicEvent(string[] message)
            : this(message, 0, null)
        {
        }

        /// <summary>
        /// Erstellt eine neue Instanz der <see cref="BasicEvent"/> Klasse.
        /// </summary>
        /// <param name="message">Einzelne Zeilen der Nachricht</param>
        /// <param name="errorNumber">Fehlernummer</param>
        /// <param name="error">Fehlerbeschreibung</param>
        public BasicEvent(string[] message, int errorNumber, string error) : base(message, errorNumber,error)
        {
            Receiver = ExtractReceiverFromMessage(message);
        }

        /// <summary>
        /// Extrahiert die Empfänger ID aus einm Event.
        /// </summary>
        /// <param name="message">Einzelne Zeilen der Nachricht</param>
        /// <returns>Empfänger ID</returns>
        private static int ExtractReceiverFromMessage(IReadOnlyList<string> message)
        {
            var value = message[0].Split(' ')[1].TrimEnd('>', ' ');
            int receiver;
            int.TryParse(value, out receiver);

            return receiver;
        }
    }
}
