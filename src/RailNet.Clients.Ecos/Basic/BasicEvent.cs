using System;
using System.IO;
using System.Linq;

namespace RailNet.Clients.Ecos.Basic
{
    public struct BasicEvent
    {
        public string[] Content;

        public string Error;
        public int ErrorNumber;

        public int Receiver;

        public DateTime Timestamp;

        public BasicEvent(int errorNumber, string error)
            : this(null, errorNumber, error)
        {
        }

        public BasicEvent(string[] content)
            : this(content, 0, null)
        {
        }

        public BasicEvent(string[] content, int errorNumber, string error)
        {
            Content = content;
            ErrorNumber = errorNumber;
            Error = error;
            Timestamp = DateTime.Now;

            Receiver = ExtractReceiverFromContent(content);
        }

        private static int ExtractReceiverFromContent(string[] content)
        {
            var value = content[0].Split(' ')[1].TrimEnd('>', ' ');
            int returnValue = 0;
            int.TryParse(value, out returnValue);

            return returnValue;
        }

        internal void ExtractError()
        {
            string footer = Content.Last();

            if (!footer.StartsWith("<END "))
                throw new InvalidDataException("No valid End found.");

            footer = footer.Substring(5, footer.Length - 6);

            var result = footer.Split(' ');

            ErrorNumber = int.Parse(result[0]);
            Error = result[1].Trim('(', ')');
        }
    }
}
