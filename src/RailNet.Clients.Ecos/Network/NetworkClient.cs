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
        private Thread _readerThread;
        private Thread _sendThread;
        private volatile bool _shouldStop = false;
        //private readonly EventWaitHandle _newMessage = new ManualResetEvent(true);
        private volatile ConcurrentBag<string> _messageList;

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
        }

        public void Disconnect()
        {
            _shouldStop = true;

            if (_tcpReader != null)
            {
                _tcpReader.Close();
                _tcpReader = null;
            }
            if (_tcpStream != null)
            {
                _tcpStream.Close();
                _tcpStream = null;
            }
            if (_tcpWriter != null)
            {
                _tcpWriter.Close();
                _tcpWriter = null;
            }
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }
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
                logger.ErrorException("Could not connect to " + host + ":" + port, ex);
                return ex.SocketErrorCode;
            }

            _tcpStream = _tcpClient.GetStream();
            _tcpWriter = new StreamWriter(_tcpStream);
            _tcpReader = new StreamReader(_tcpStream);
            _tcpWriter.AutoFlush = true;

            _readerThread.Start();
            _sendThread.Start();

            logger.Debug("Connected to {0}:{1}", host, port);

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
            _shouldStop = false;
            _tcpClient = new TcpClient(AddressFamily.InterNetwork);

            result = new List<string>();
            _messageList = new ConcurrentBag<string>();

            _readerThread = new Thread(async () =>
            {
                while (!_shouldStop)
                {
                    try
                    {
                        await ListenAsync();
                    }
                    catch (Exception)
                    {

                    }
                }
            });

            _sendThread = new Thread(async () =>
            {
                while (!_shouldStop)
                {
                    //Return requested?
                    if (_shouldStop)
                        return;

                    //Warten
                    if (_messageList.Count == 0)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    //Versende Messages
                    string message;
                    if (_messageList.TryTake(out message))
                    {
                        await WriteAsync(message);
                        await Task.Delay(_messageDelay);
                    }
                    else
                    {
                        logger.Error("Konnte keine Nachricht zum Versenden aus ConcurrentBag holen!");
                    }
                }
            });
        }

        ~NetworkClient()
        {
            Disconnect();
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
