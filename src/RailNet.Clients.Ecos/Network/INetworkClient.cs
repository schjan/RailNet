using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos.Network
{
    public interface INetworkClient
    {
        /// <summary>
        /// Delay in ms between to messages (200 is default)
        /// </summary>
        int MessageDelay { get; set; }

        /// <summary>
        /// Value if Client is Connected to ECoS
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Connects async to ECoS
        /// </summary>
        /// <returns>SocketError Error</returns>
        Task<SocketError> ConnectAsync(string host);

        /// <summary>
        /// Connects async to ECoS
        /// </summary>
        /// <returns>SocketError Error</returns>
        Task<SocketError> ConnectAsync(string host, int port);

        /// <summary>
        /// Puts a Message into MQ to send to ECoS
        /// </summary>
        /// <param name="text">Message</param>
        void SendMessage(string text);

        /// <summary>
        /// Sends a Message directly to Server without MQ.
        /// </summary>
        /// <param name="text">Message</param>
        /// <returns>awaitable Task</returns>
        Task WriteAsync(string text);

        /// <summary>
        /// Event gets fired when ECoS sends Message to the Client.
        /// </summary>
        event MessageReceivedEventHandler MessageReceivedEvent;

        
    }

    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
    
    /// <summary>
    /// EventArgs for MessageReceivedEvent
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string[] content)
        {
            Content = content;
        }

        public string[] Content { get; private set; }
    }
}
