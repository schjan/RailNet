using System;

namespace RailNet.Clients.Ecos.Basic
{
    public struct BasicAntwort
    {
        public string Befehl;
        public string[] Content;

        public string Error;
        public int ErrorNumber;

        public DateTime Timestamp;

        public BasicAntwort(string[] content)
            : this(content, null, 0, null)
        {
        }

        public BasicAntwort(int errorNumber, string error)
            : this(null, null, errorNumber, error)
        {
        }

        public BasicAntwort(string[] content, string befehl)
            : this(content, befehl, 0, null)
        {
        }

        public BasicAntwort(string[] content, string befehl, int errorNumber, string error)
        {
            Content = content;
            ErrorNumber = errorNumber;
            Error = error;
            Befehl = befehl;
            Timestamp = DateTime.Now;
        }
    }
}
