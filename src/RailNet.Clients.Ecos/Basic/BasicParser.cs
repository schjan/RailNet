using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RailNet.Clients.Ecos.Basic
{
    /// <summary>
    /// Parser für ESUs Nachrichteninhalt.
    /// </summary>
    internal class BasicParser
    {
        /// <summary>
        /// Teilt Nachrichtenzeilen in einzelne Elemente.
        /// </summary>
        /// <param name="message">Einzelne Zeilen einer Nachricht</param>
        /// <returns>Array von einzelnen strings einer Zeile.</returns>
        internal static IEnumerable<string[]> ParseContent(string[] message)
        {
            foreach (var s in message)
            {
                if (s.StartsWith("<"))
                    continue;
                yield return s.Split(' ', ']', '[');
            }
        }

        /// <summary>
        /// Versucht falls vorhanden aus einer einzelnen Zeile ein bestimmtes Parameter auszulesen.
        /// </summary>
        /// <param name="param">Parametername</param>
        /// <param name="message">Nachricht</param>
        /// <param name="hasQuotation">Gibt an ob der Wert von Anführungszeichen umgeben ist.</param>
        /// <returns>Wert oder leeren String.</returns>
        public static string TryGetParameterFromContent(string param, string message, bool hasQuotation = false)
        {
            Match match;
            if (hasQuotation)
                match = Regex.Match(message, @"(?<=" + param + @"\[\"")(?:\\.|[^\\])*?(?=\""\])");
            else
                match = Regex.Match(message, @"(?<=" + param + @"\[)(?:\\.|[^\\])*?(?=\])");

            if (!match.Success)
                return string.Empty;

            return match.Value;
        }
    }
}
