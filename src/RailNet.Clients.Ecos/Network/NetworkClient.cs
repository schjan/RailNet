using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace RailNet.Clients.Ecos.Network
{
    internal class NetworkClient : INetworkClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _tcpStream;
        private StreamReader _tcpReader;
        private StreamWriter _tcpWriter;
        private readonly Thread _readerThread;
        private readonly Thread _sendThread;
        private volatile bool _shouldStop = false;
        private readonly EventWaitHandle _newMessage = new ManualResetEvent(true);
        private readonly ConcurrentBag<string> _messageList;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private int _messageDelay = 200;
        /// <summary>
        /// Delay in ms between to messages (200 is default)
        /// </summary>
        public int MessageDelay
        {
            get { return _messageDelay; }
            set
            {
                if (value >= 0)
                    _messageDelay = value;
            }
        }

        /// <summary>
        /// Client to directly connect to ECoS via TCP
        /// </summary>
        public NetworkClient()
        {
            CreateTcpClient();

            result = new List<string>();
            _messageList = new ConcurrentBag<string>();

            _readerThread = new Thread(async () =>
            {
                while (!_shouldStop)
                {
                    await ListenAsync();
                }
            });

            _sendThread = new Thread(async () =>
            {
                while (!_shouldStop)
                {
                    //Warten
                    _newMessage.WaitOne();

                    //Return requested?
                    if (_shouldStop)
                        return;
                    
                    //Versende Messages
                    string message;
                    if (_messageList.TryTake(out message))
                    {
                        await WriteAsync(message);
                        await Task.Delay(_messageDelay);
                    }
                    else
                    {
                        lock (_newMessage)
                        {
                            if (_newMessage.WaitOne(1) == false)
                                _newMessage.Reset();
                        }
                        
                    }
                }
            });
        }

        /// <summary>
        /// Value if Client is Connected to ECoS
        /// </summary>
        public bool Connected
        {
            get { return _tcpClient != null && _tcpClient.Connected; }
        }

        /// <summary>
        /// Connects async to ECoS
        /// </summary>
        /// <returns>SocketError Error</returns>
        public async Task<SocketError> ConnectAsync(string host)
        {
            return await ConnectAsync(host, 15471);
        }

        /// <summary>
        /// Connects async to ECoS
        /// </summary>
        /// <returns>SocketError Error</returns>
        public async Task<SocketError> ConnectAsync(string host, int port)
        {
            if (_tcpClient == null)
                CreateTcpClient();

            Debug.Assert(_tcpClient != null, "tcpClient != null");
            if (_tcpClient.Connected)
                return SocketError.IsConnected;


            try
            {
                await _tcpClient.ConnectAsync(host, port);
            }
            catch (SocketException ex)
            {
                return ex.SocketErrorCode;
            }

            _tcpStream = _tcpClient.GetStream();
            _tcpWriter = new StreamWriter(_tcpStream);
            _tcpReader = new StreamReader(_tcpStream);
            _tcpWriter.AutoFlush = true;

            _readerThread.Start();
            _sendThread.Start();

            return SocketError.Success;
        }


        private IList<string> result;
        private async Task ListenAsync()
        {
            var message = await _tcpReader.ReadLineAsync();

            logger.Trace(message);
            
            result.Add(message);
            if (message.StartsWith("<END"))
                FlushMessage();
        }

        private void FlushMessage()
        {
            RaiseMessageReceivedEvent(result.ToArray());
            result.Clear();
        }

        /// <summary>
        /// Puts a Message into MQ to send to ECoS
        /// </summary>
        /// <param name="text">Message</param>
        public void SendMessage(string text)
        {
            _messageList.Add(text);
            _newMessage.Set();
        }

        /// <summary>
        /// Sends a Message directly to Server without MQ.
        /// </summary>
        /// <param name="text">Message</param>
        /// <returns>awaitable Task</returns>
        public async Task WriteAsync(string text)
        {
            await _tcpWriter.WriteLineAsync(text);
            await _tcpWriter.FlushAsync();

            logger.Trace("Send: {0}", text);
        }

        private void CreateTcpClient()
        {
            _tcpClient = new TcpClient(AddressFamily.InterNetwork);
        }

        ~NetworkClient()
        {
            _shouldStop = true;

            _tcpReader.Close();
            _tcpReader = null;
            _tcpStream.Close();
            _tcpStream = null;
            _tcpWriter.Close();
            _tcpWriter = null;

            _tcpClient.Close();
            _tcpClient = null;
        }

        /// <summary>
        /// Event gets fired when ECoS sends Message to the Client.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceivedEvent;

        protected void RaiseMessageReceivedEvent(string[] content)
        {
            if (MessageReceivedEvent != null)
                MessageReceivedEvent(this, new MessageReceivedEventArgs(content));
        }

    }
}
