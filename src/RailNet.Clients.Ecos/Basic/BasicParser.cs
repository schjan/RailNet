using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
