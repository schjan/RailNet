using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using RailNet.Clients.Ecos.Network;
using TinyIoC;

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
        private readonly IDictionary<string, BasicResponse> _antworten;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly INetworkClient _networkClient;

        private IObservable<BasicResponse> incomingMessages;
        // TODO: incomingEvents
        //private IObservable<BasicEvent> incomingEvents; 
        
        private bool _strictFailureStrategy = true;
        private DateTime _lastCheck;

        /// <summary>
        /// Default constructor gets the INetworkClient from IoC
        /// </summary>
        public NachrichtenDispo()
            : this(TinyIoCContainer.Current.Resolve<INetworkClient>())
        {
            
        }

        public NachrichtenDispo(INetworkClient networkClient)
        {
            _networkClient = networkClient;

            // Rx :)
            incomingMessages = Observable.FromEventPattern<MessageReceivedEventHandler, MessageReceivedEventArgs>(
                h => _networkClient.MessageReceivedEvent += h,
                h => _networkClient.MessageReceivedEvent -= h)
                .Select(x => x.EventArgs.Content)
                .Where(ValidateMessage)
                .Where(x => GetMessageType(x) == MessageType.Reply)
                .Select(ParseResponse);
        }

        public NachrichtenDispo(INetworkClient networkClient, bool strictFailureStrategy) : this(networkClient)
        {
            _strictFailureStrategy = strictFailureStrategy;
        }

       /// <summary>
       /// Von ausserhalb Aufgerufene Methode zum senden einer Nachricht.
       /// Sendet eine Nachricht und awaited die Antwort zu der Nachricht.
       /// </summary>
       /// <param name="befehl"></param>
       /// <returns></returns>
        public Task<BasicResponse> SendCommandAsync(string command)
        {
            if (!_networkClient.Connected)
                throw new IOException("Client nicht verbunden!");

            var result = incomingMessages
                .Where(reply => reply.Command == command)
                .Timeout(TimeSpan.FromSeconds(2))
                .Take(1)
                .ToTask();

            _networkClient.SendMessage(command);

            return result;
        }

        private BasicResponse ParseResponse(string[] message)
        {
            return new BasicResponse(message);
        }

        // TODO: Mehr Validation noetig?
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

        private MessageType GetMessageType(string[] message)
        {
            if (message.Length < 3)
                return MessageType.Undefined;

            if (message[0].StartsWith("<REPLY "))
                return MessageType.Reply;

            if (message[0].StartsWith("<EVENT "))
                return MessageType.Event;

            return MessageType.Undefined;
        }

        private void ExtractError(ref BasicResponse a)
        {
            string footer = a.Content.Last();

            if (!footer.StartsWith("<END "))
                throw new Exception("This is not the End");

            footer = footer.Substring(5, footer.Length - 6);

            var result = footer.Split(' ');

            a.ErrorNumber = int.Parse(result[0]);
            a.Error = result[1].Trim('(', ')');
        }

        private void ThrowException(Exception e)
        {
            ThrowException(e.Message, e);
        }

        private void ThrowException(string message, Exception e)
        {
            logger.ErrorException(e.Message, e);

            if (_strictFailureStrategy)
                throw e;
        }
    }

    internal enum MessageType
    {
        Undefined = 0,
        Reply,
        Event
    }
}
