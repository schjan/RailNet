using System;
using System.IO;
using System.Linq;

namespace RailNet.Clients.Ecos.Basic
{
    public struct BasicResponse
    {
        public string Command;
        public string[] Content;

        public string Error;
        public int ErrorNumber;

        public DateTime Timestamp;

        public BasicResponse(string[] content)
            : this(content, GetCommandByContent(content), 0, null)
        {
        }

        public BasicResponse(int errorNumber, string error)
            : this(null, null, errorNumber, error)
        {
        }

        public BasicResponse(string[] content, string command)
            : this(content, command, 0, null)
        {
        }

        public BasicResponse(string[] content, string command, int errorNumber, string error)
        {
            Content = content;
            ErrorNumber = errorNumber;
            Error = error;
            Command = command;
            Timestamp = DateTime.Now;
        }

        private static string GetCommandByContent(string[] content)
        {
            return content[0].Substring(7, content[0].Length - 8);
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
