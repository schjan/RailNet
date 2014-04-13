using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using RailNet.Clients.Ecos.Network;

namespace RailNet.Clients.Ecos.Basic
{

    /// <summary>
    /// NachrichtenDispo wird vom BasicClient genutzt um auf den NetworkClient zuzugreifen.
    /// Der NachrichtenDispo ordnet gesendeten Befehlen die passenden Antworten zu. Im NachrichtenDispo
    /// findet das "awaiten" der Antwort auf Nachrichten statt.
    /// </summary>
    public class NachrichtenDispo : INachrichtenDispo
    {
        private readonly IDictionary<string, EventWaitHandle> _currentQuerys;
        private readonly IDictionary<string, BasicAntwort> _antworten;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly INetworkClient _networkClient;
        
        private bool strictFailureStrategy = true;
        private DateTime _lastCheck;

        /// <summary>
        /// Default constructor
        /// </summary>
        public NachrichtenDispo() : this(new NetworkClient())
        {
        }

        public NachrichtenDispo(INetworkClient networkClient)
        {
            _networkClient = networkClient;
            _currentQuerys = new ConcurrentDictionary<string, EventWaitHandle>();
            _antworten = new ConcurrentDictionary<string, BasicAntwort>();
            _lastCheck = DateTime.Now;
            _networkClient.MessageReceivedEvent += Client_MessageReceivedEvent;
        }

       private void Client_MessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            VerarbeiteMessage(e.Content);
        }

       #region Public

       /// <summary>
       /// Von ausserhalb Aufgerufene Methode zum senden einer Nachricht.
       /// Sendet eine Nachricht und awaited die Antwort zu der Nachricht.
       /// </summary>
       /// <param name="befehl"></param>
       /// <returns></returns>
       public async Task<BasicAntwort> SendeBefehlAsync(string befehl)
       {
           await SendeBefehlAwaitResponse(befehl);

           return FindAnswer(befehl);
       }

       /// <summary>
       /// Findet die Serverantwort zum eingegebenen Befehl.
       /// Wenn keine Antwort gefunden wurde, wird eine Error Antwort mit Error 404 ausgegeben.
       /// </summary>
       /// <param name="befehl"></param>
       /// <returns></returns>
       private BasicAntwort FindAnswer(string befehl)
       {
           lock (_antworten)
           {
               if (!_antworten.ContainsKey(befehl))
                   return new BasicAntwort { Error = "Antwort nicht gefunden!", ErrorNumber = 404 };

               BasicAntwort a = _antworten.First(x => x.Key == befehl).Value;
               if (a.Content == null)
                   return a;

               ExtractError(ref a);

               Console.WriteLine("Antwort auf " + a.Befehl);

               return a;
           }
       }

        /// <summary>
        /// Sendet den angegebenen Befehl an den Server und wartet, bis der Server auf den Befehl antwortet.
        /// Wird das Timeout überschritten wird eine Antwort mit "Timeout" generiert.
        /// </summary>
        /// <param name="befehl"></param>
        /// <returns></returns>
        private async Task SendeBefehlAwaitResponse(string befehl)
        {
            if (!_networkClient.Connected)
                throw new IOException("Client nicht verbunden!");

            var e = new EventWaitHandle(false, EventResetMode.ManualReset);
            var signal = false;


            if (_currentQuerys.ContainsKey(befehl))
                e = _currentQuerys.First(x => x.Key == befehl).Value;
            else
                _currentQuerys.Add(befehl, e);

            _networkClient.SendMessage(befehl);

            await Task.Run(() => signal = e.WaitOne(8000));

            e = null;

            if (!signal)
            {
                _currentQuerys.Remove(befehl);
                _antworten.Add(befehl, new BasicAntwort(null, befehl, 500, "Timeout!"));
            }
        }

        #endregion

       #region Message Verarbeiten

       private void VerarbeiteMessage(string[] message)
        {
            logger.Trace("Verarbeite {0}", message[0]);

            if (!HasBeginAndEnd(message))
            {
                ThrowException(
                    new InvalidDataException("Message konnte nicht verarbeitet werden und passt nicht in das Message Schema!\r\n"
                                             + message[0]));
            }

            if (message[0].StartsWith("<REPLY "))
            {
                string header = message[0].Substring(7, message[0].Length - 8);

                _antworten.Remove(header);
                _antworten.Add(header, new BasicAntwort(message, header));

                if (_currentQuerys.All(x => x.Key != header))
                    return;

                var query = _currentQuerys.First(x => x.Key == header);
                query.Value.Set(); // Can GC collect WaitHandles with this code?
                _currentQuerys.Remove(query);
            }
            else if (message[0].StartsWith("<EVENT "))
            {
            }
            else
            {
                ThrowException(new InvalidDataException("Unbekannter Nachrichtentyp!",
                    new Exception("Data: " + message[0])));
            }

            if(_lastCheck < DateTime.Now.AddSeconds(-10))
                ClearAntwortenCache();
        }

        private void ClearAntwortenCache()
        {
            lock (_antworten)
            {
                string[] keys = _antworten.Keys.ToArray();

                foreach (var key in keys)
                {
                    BasicAntwort a;

                    if (_antworten.TryGetValue(key, out a))
                    {
                        if (a.Timestamp.Second < DateTime.Now.Second - 10)
                            _antworten.Remove(key);
                    }
                }

                _lastCheck = DateTime.Now;
                logger.Debug(() => "Cleared cache");
            }
        }

        // Mehr Validation noetig?
        private bool ValidateMessage(string[] message)
        {
            return HasBeginAndEnd(message);
        }
        
        private bool HasBeginAndEnd(string[] message)
        {
            bool isValid = true;


            if (!message[0].StartsWith("<") || !message[0].EndsWith(">"))
                isValid = false;

            if (!message.Last().StartsWith("<END"))
                isValid = false;

            return isValid;
        }

        private void ExtractError(ref BasicAntwort a)
        {
            string footer = a.Content.Last();

            if (!footer.StartsWith("<END "))
                throw new Exception("This is not the End");

            footer = footer.Substring(5, footer.Length - 6);

            var result = footer.Split(' ');

            a.ErrorNumber = int.Parse(result[0]);
            a.Error = result[1].Trim('(', ')');
        }

        #endregion

        private void ThrowException(Exception e)
        {
            ThrowException(e.Message, e);
        }

        private void ThrowException(string message, Exception e)
        {
            logger.ErrorException(e.Message, e);

            if (strictFailureStrategy)
                throw e;
        }
    }
}
