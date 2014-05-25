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
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly INetworkClient _networkClient;

        private readonly IObservable<BasicResponse> incomingMessages;
        private readonly IObservable<BasicEvent> incomingEvents;

        /// <summary>
        /// Eine IObservable von Serverseitigen Events
        /// </summary>
        public IObservable<BasicEvent> IncomingEvents
        {
            get { return incomingEvents; }
        }

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

            incomingEvents = Observable.FromEventPattern<MessageReceivedEventHandler, MessageReceivedEventArgs>(
                h => _networkClient.MessageReceivedEvent += h,
                h => _networkClient.MessageReceivedEvent -= h)
                .Select(x => x.EventArgs.Content)
                .Where(ValidateMessage)
                .Where(x => GetMessageType(x) == MessageType.Event)
                .Select(ParseEvent);
        }

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
            var response = new BasicResponse(message);
            
            response.ExtractError();

            return response;
        }

        private BasicEvent ParseEvent(string[] message)
        {
            var Event = new BasicEvent(message);

            Event.ExtractError();

            return Event;
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
    }

    internal enum MessageType
    {
        Undefined = 0,
        Reply,
        Event
    }
}
