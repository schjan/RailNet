namespace RailNet.Clients.Ecos.Basic
{
    public class BasicResponse : BasicMessage
    {
        public string Command;

        public BasicResponse(string[] message)
            : this(message, GetCommandByMessage(message), 0, null)
        {
        }

        public BasicResponse(int errorNumber, string error)
            : this(null, null, errorNumber, error)
        {
        }

        public BasicResponse(string[] message, string command)
            : this(message, command, 0, null)
        {
        }

        public BasicResponse(string[] message, string command, int errorNumber, string error)
            : base(message, errorNumber, error)
        {
            Command = command;
        }

        private static string GetCommandByMessage(string[] content)
        {
            return content[0].Substring(7, content[0].Length - 8);
        }
    }
}
