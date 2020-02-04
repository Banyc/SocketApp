using System.Linq;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections.Generic;

namespace SocketApp
{
    public enum SocketRole
    {
        Listener,
        Client,
    }

    public class SocketSendEventArgs
    {
        public SocketSendEventArgs(SendStateObject state, SockBase handler) { State = state; Handler = handler; }
        public SockBase Handler { get; }
        public SendStateObject State { get; }
    }

    public class SocketAcceptEventArgs
    {
        public SocketAcceptEventArgs(AcceptStateObject state, SockBase handler) { State = state; Handler = handler; }
        public SockBase Handler { get; }
        public AcceptStateObject State { get; }
    }
    public class SocketConnectEventArgs
    {
        public SocketConnectEventArgs(ConnectStateObject state, SockBase handler) { State = state; Handler = handler; }
        public SockBase Handler { get; }
        public ConnectStateObject State { get; }
    }

    public class SocketShutdownBeginEventArgs
    {
        public SocketShutdownBeginEventArgs(bool isShutdown) { IsShutdown = isShutdown; }
        public bool IsShutdown { get; }
    }

    public class SocketReceiveEventArgs
    {
        public SocketReceiveEventArgs(byte[] buffer) { Buffer = buffer; }
        public byte[] Buffer;
    }

    // State object for reading client data asynchronously  
    public class ReadStateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        public SockBase Source = null;  // caller
        // Size of receive buffer.  
        public const int BufferSize = 1 * 1024 * 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // buffer manager
    }

    public class ConnectStateObject
    {
        public Socket workSocket = null;
        public IPEndPoint endPoint = null;
        public int timesToTry = 1;
        public SocketError errorType;
        public SockBase.SocketConnectEventHandler externalCallback = null;
        public object externalCallbackState = null;
    }

    public class AcceptStateObject
    {
        public Socket workSocket = null;
        public SockBase.SocketAcceptEventHandler externalCallback = null;
        public object externalCallbackState = null;
    }

    public class SendStateObject
    {
        public Socket workSocket = null;
        public SockBase.SocketSendEventHandler externalCallback = null;
        public object externalCallbackState = null;
    }

    // socket base to provide basic asynchronous interface of system socket
    public class SockBase
    {
        public delegate void SocketSendEventHandler(object sender, SocketSendEventArgs e);
        public event SocketSendEventHandler SocketSendEvent;
        public delegate void SocketAcceptEventHandler(object sender, SocketAcceptEventArgs e);
        public event SocketAcceptEventHandler SocketAcceptEvent;
        public delegate void SocketConnectEventHandler(object sender, SocketConnectEventArgs e);
        public event SocketConnectEventHandler SocketConnectEvent;
        public delegate void SocketShutdownBeginEventHandler(object sender, SocketShutdownBeginEventArgs e);
        public event SocketShutdownBeginEventHandler SocketShutdownBeginEvent;
        public delegate void SocketReceiveEventHandler(object sender, SocketReceiveEventArgs e);
        public event SocketReceiveEventHandler SocketReceiveEvent;
        Socket _socket;
        public SocketRole Role { get; }
        public bool IsHost { get; }  // is the socket is born from a listener or is itself a listener
        public bool IsConnected = false;  // has the socket established a connection
        private bool _isReceiveStart = false;  // WORKAROUND

        public SockBase(Socket s, SocketRole r, bool isHost)
        {
            _socket = s;
            this.Role = r;
            this.IsHost = isHost;
        }

        public Socket GetSocket()
        {
            return _socket;
        }

        // async
        public void StartSend(byte[] data, SocketSendEventHandler externalCallback = null, object externalCallbackState = null)
        {
            SendStateObject state = new SendStateObject();
            state.workSocket = _socket;
            state.externalCallback = externalCallback;
            state.externalCallbackState = externalCallbackState;
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None,
                new System.AsyncCallback(SendCallback), state);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                SendStateObject state = (SendStateObject) ar.AsyncState;
                Socket handler = state.workSocket;
                handler.EndSend(ar);

                SocketSendEvent?.Invoke(this, new SocketSendEventArgs(state, this));
                if (state.externalCallback != null)
                    state.externalCallback(this, new SocketSendEventArgs(state, this));
            }
            catch (ObjectDisposedException) { }  // socket closed
        }

        // async Accept
        public void StartAccept(SocketAcceptEventHandler externalCallback = null, object externalCallbackState = null)
        {
            AcceptStateObject state = new AcceptStateObject();
            state.workSocket = _socket;
            state.externalCallback = externalCallback;
            state.externalCallbackState = externalCallbackState;
            _socket.BeginAccept(
                new System.AsyncCallback(AcceptCallback), state);
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                AcceptStateObject state = (AcceptStateObject) ar.AsyncState;
                // Get the socket that handles the client request.  
                Socket listener = state.workSocket;
                Socket handler = listener.EndAccept(ar);

                SockBase sockBase = new SockBase(handler, SocketRole.Client, true);
                sockBase.IsConnected = true;
                sockBase.StartReceive();

                SocketAcceptEvent?.Invoke(this, new SocketAcceptEventArgs(state, sockBase));
                if (state.externalCallback != null)
                    state.externalCallback(this, new SocketAcceptEventArgs(state, sockBase));

                listener.BeginAccept(
                    new System.AsyncCallback(AcceptCallback), state);
            }
            catch (ObjectDisposedException) { }  // listener closed
        }

        // async Connect
        public void StartConnect(IPEndPoint ep, int timesToTry, SocketConnectEventHandler externalCallback = null, object externalCallbackState = null)
        {
            if (this.IsConnected == true)
                return;
            ConnectStateObject state = new ConnectStateObject();
            state.workSocket = _socket;
            state.endPoint = ep;
            state.timesToTry = timesToTry;
            state.externalCallback = externalCallback;
            state.externalCallbackState = externalCallbackState;
            _socket.BeginConnect(ep,
                new System.AsyncCallback(ConnectCallback), state);
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            ConnectStateObject state = (ConnectStateObject)ar.AsyncState;
            try
            {
                state.workSocket.EndConnect(ar);
                this.IsConnected = true;
                StartReceive();
            }
            catch (SocketException ex)
            {
                state.errorType = (SocketError)ex.ErrorCode;
                --state.timesToTry;
                if (state.timesToTry <= 0)
                    return;

                state.workSocket.BeginConnect(state.endPoint,
                    new System.AsyncCallback(ConnectCallback), state);
            }
            finally
            {
                SocketConnectEvent?.Invoke(this, new SocketConnectEventArgs(state, this));
                if (state.externalCallback != null)
                    state.externalCallback(this, new SocketConnectEventArgs(state, this));
            }
        }

        public void StartReceive()
        {
            if (!_isReceiveStart)
            {
                _isReceiveStart = true;
                ReadStateObject state = new ReadStateObject();
                state.workSocket = _socket;
                state.Source = this;
                _socket.BeginReceive(state.buffer, 0, ReadStateObject.BufferSize, SocketFlags.None,
                    new AsyncCallback(ReadCallback), state);
            }
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            ReadStateObject state = (ReadStateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            try
            {
                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                // handle FIN
                if (bytesRead == 0)
                {
                    state.Source.Shutdown();
                    return;
                }

                if (bytesRead > 0)
                {
                    // raise event
                    state.Source.SocketReceiveEvent?.Invoke(state.Source, new SocketReceiveEventArgs(state.buffer.Take(bytesRead).ToArray()));
                }

                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, ReadStateObject.BufferSize, SocketFlags.None,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (SocketException ex)
            {
                // disconnection detected
                switch ((SocketError)ex.ErrorCode)
                {
                    case SocketError.ConnectionReset:
                        state.Source.Shutdown();
                        break;
                    default:
                        state.Source.Shutdown();
                        break;
                }
            }
            catch (ObjectDisposedException) { }  // the socket has been disposed.
        }

        public void Shutdown()
        {
            SocketShutdownBeginEvent?.Invoke(this, new SocketShutdownBeginEventArgs(true));

            try
            {
                if (this.Role == SocketRole.Client)
                    _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (Exception) { }  // TODO: Specify details
        }
    }
}
