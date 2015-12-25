using System;
using System.Linq;

namespace RailNet.Clients.Ecos.Basic
{
    /// <summary>
    /// Message wie es von der ECOS an den <see cref="BasicClient">Client</see> gesendet wird.
    /// </summary>
    public abstract class BasicMessage
    {
        /// <summary>
        /// Einzelne Zeilen der Nachricht
        /// </summary>
        public string[] Content { get; }

        /// <summary>
        /// Fehler Beschreibung. Leer wenn kein Fehler.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Fehlernummer. 0 wenn kein Fehler.
        /// </summary>
        public int ErrorNumber { get; private set; }

        /// <summary>
        /// Gibt an ob die Nachricht eine Fehlermeldung enthält.
        /// </summary>
        public bool HasError => ErrorNumber != 0;

        /// <summary>
        /// Eingangszeit der Nachricht.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Erstellt eine neue Instanz von <see cref="BasicMessage"/>
        /// </summary>
        /// <param name="message">Einzelne Zeilen der Nachricht</param>
        /// <param name="errorNumber">Fehlernummer</param>
        /// <param name="error">Fehlerbeschreibung</param>
        protected BasicMessage(string[] message, int errorNumber, string error)
        {
            Content = ExtractContentOfMessage(message);

            ErrorNumber = errorNumber;
            Error = error;
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Extrahiert den Inhalt (Fehlermeldung) einer Nachricht und gibt die Nachricht ohne Header und Footer zurück.
        /// </summary>
        /// <param name="message">Einzelne Zeilen der Nachricht</param>
        /// <returns>Nachricht ohne Header und Footer.</returns>
        internal string[] ExtractContentOfMessage(string[] message)
        {
            if (!message[0].StartsWith("<"))
                return message;

            var footer = message.Last();

            if (!footer.StartsWith("<END "))
                throw new InvalidDataReceivedException("No valid End found.");

            footer = footer.Substring(5, footer.Length - 6);

            var result = footer.Split(' ');

            ErrorNumber = int.Parse(result[0]);
            Error = result[1].Trim('(', ')');

            var content = new string[message.Length - 2];
            for (var i = 0; i < message.Length - 2; i++)
            {
                content[i] = message[i + 1];
            }

            return content;
        }
    }
}
