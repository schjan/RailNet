using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos
{
    public class InvalidDataReceivedException : Exception
    {
        public InvalidDataReceivedException(string message) : base(message)
        {
        }
    }
}
