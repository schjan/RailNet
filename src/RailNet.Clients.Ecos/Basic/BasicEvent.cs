using System;
using System.IO;
using System.Linq;

namespace RailNet.Clients.Ecos.Basic
{
    public class BasicEvent : BasicMessage
    {
        public int Receiver;
        
        public BasicEvent(int errorNumber, string error)
            : this(null, errorNumber, error)
        {
        }

        public BasicEvent(string[] message)
            : this(message, 0, null)
        {
        }

        public BasicEvent(string[] message, int errorNumber, string error) : base(message, errorNumber,error)
        {
            Receiver = ExtractReceiverFromMessage(message);
        }

        private static int ExtractReceiverFromMessage(string[] content)
        {
            var value = content[0].Split(' ')[1].TrimEnd('>', ' ');
            int receiver;
            int.TryParse(value, out receiver);

            return receiver;
        }
    }
}
