using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using RailNet.Clients.Ecos.Network;

namespace RailNet.Clients.Ecos.Basic
{

    /// <summary>
    /// NachrichtenDispo wird vom BasicClient genutzt um auf den NetworkClient zuzugreifen.
    /// Der NachrichtenDispo ordnet gesendeten Befehlen die passenden Antworten zu. Im NachrichtenDispo
    /// findet das "awaiten" der Antwort auf Nachrichten statt.
    /// </summary>
    internal class NachrichtenDispo : INachrichtenDispo
    {
        private readonly INetworkClient _networkClient;

        private readonly IObservable<BasicResponse> _incomingMessages;
        private readonly IObservable<BasicEvent> _incomingEvents;

        /// <summary>
        /// Eine IObservable von Serverseitigen Events
        /// </summary>
        public IObservable<BasicEvent> IncomingEvents => _incomingEvents;

        public NachrichtenDispo(INetworkClient networkClient)
        {
            _networkClient = networkClient;

            // Rx :)
            _incomingMessages = Observable.FromEventPattern<MessageReceivedEventHandler, MessageReceivedEventArgs>(
                h => _networkClient.MessageReceivedEvent += h,
                h => _networkClient.MessageReceivedEvent -= h)
                .Select(x => x.EventArgs.Content)
                .Where(ValidateMessage)
                .Where(x => GetMessageType(x) == MessageType.Reply)
                .Select(ParseResponse);

            _incomingEvents = Observable.FromEventPattern<MessageReceivedEventHandler, MessageReceivedEventArgs>(
                h => _networkClient.MessageReceivedEvent += h,
                h => _networkClient.MessageReceivedEvent -= h)
                .Select(x => x.EventArgs.Content)
                .Where(ValidateMessage)
                .Where(x => GetMessageType(x) == MessageType.Event)
                .Select(ParseEvent);
        }

#if DEBUG
        private const int Timeout = 5;
#else
        private const int Timeout = 2;
#endif

       /// <summary>
       /// Von ausserhalb Aufgerufene Methode zum senden einer Nachricht.
       /// Sendet eine Nachricht und liefert die Antwort der Nachricht asynchron.
       /// </summary>
       /// <param name="command"></param>
       /// <returns></returns>
        public Task<BasicResponse> SendCommandAsync(string command)
        {
            if (!_networkClient.Connected)
                throw new IOException("Client nicht verbunden!");

            var result = _incomingMessages
                .Where(reply => reply.Command == command)
                .Timeout(TimeSpan.FromSeconds(Timeout))
                .Take(1)
                .ToTask();

            _networkClient.SendMessage(command);

            return result;
        }

        private static BasicResponse ParseResponse(string[] message)
        {
            return new BasicResponse(message);
        }

        private static BasicEvent ParseEvent(string[] message)
        {
            return new BasicEvent(message);
        }

        // TODO: Mehr Validation noetig?
        private static bool ValidateMessage(string[] message)
        {
            return HasBeginAndEnd(message);
        }
        
        private static bool HasBeginAndEnd(IReadOnlyList<string> message)
        {
            return message[0].StartsWith("<") && message[0].EndsWith(">") && message.Last().StartsWith("<END");
        }

        private static MessageType GetMessageType(IReadOnlyList<string> message)
        {
            if (message.Count < 2)
                return MessageType.Undefined;

            if (message[0].StartsWith("<REPLY "))
                return MessageType.Reply;

            if (message[0].StartsWith("<EVENT "))
                return MessageType.Event;

            return MessageType.Undefined;
        }
    }

    internal enum MessageType
    {
        Undefined = 0,
        Reply,
        Event
    }
}
