using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RailNet.Clients.Ecos.Basic
{
    internal class BasicParser
    {
        internal static IEnumerable<string[]> ParseContent(string[] content)
        {
            foreach (var s in content)
            {
                if (s.StartsWith("<"))
                    continue;
                yield return s.Split(' ', ']', '[');
            }
        }

        public static string TryGetParameterFromContent(string param, string content, bool hasQuotation = false)
        {
            Match match;
            if (hasQuotation)
                match = Regex.Match(content, @"(?<=" + param + @"\[\"")(?:\\.|[^\\])*?(?=\""\])");
            else
                match = Regex.Match(content, @"(?<=" + param + @"\[)(?:\\.|[^\\])*?(?=\])");

            if (!match.Success)
                return string.Empty;

            return match.Value;
        }
    }
}
