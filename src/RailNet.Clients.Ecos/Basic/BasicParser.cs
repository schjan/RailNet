using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos.Basic
{
    internal class BasicParser
    {
        internal static IEnumerable<string[]> ParseContent(string[] content)
        {
            foreach (var s in content)
            {
                if(s.StartsWith("<"))
                    continue;
                yield return s.Split(' ', ']', '[');
            }
        }

        public static string TryGetParameterFromContent(string param, string content)
        {
            var match =
                Regex.Match(content, @"(?<=" + param + @"\[)(?:\\.|[^\\])*(?=\])");

            if (!match.Success)
                return string.Empty;

            return match.Value;
        }
    }
}
