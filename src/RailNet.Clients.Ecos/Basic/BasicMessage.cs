using System;
using System.Linq;

namespace RailNet.Clients.Ecos.Basic
{
    public abstract class BasicMessage
    {
        public string[] Content { get; }

        public string Error { get; private set; }
        public int ErrorNumber { get; private set; }
        public bool HasError => ErrorNumber != 0;

        public DateTime Timestamp { get; }

        protected BasicMessage(string[] message, int errorNumber, string error)
        {
            Content = ExtractContentOfMessage(message);

            ErrorNumber = errorNumber;
            Error = error;
            Timestamp = DateTime.Now;
        }

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
