using System.Net.Sockets;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace SocketApp
{
    public enum SocketRole
    {
        Listener,
        Client,
    }

    public class SocketShutdownEventArgs
    {
        public SocketShutdownEventArgs(bool isShutdown) { IsShutdown = isShutdown; }
        public bool IsShutdown { get; }
    }

    public class SocketReceiveEventArgs
    {
        public SocketReceiveEventArgs(BufferMgr bufferMgr) { BufferMgr = bufferMgr; }
        public BufferMgr BufferMgr;
    }

    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        public SockMgr Source = null;  // caller
        // Size of receive buffer.  
        public const int BufferSize = 65535;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // buffer manager
        public BufferMgr bufferMgr = new BufferMgr();
    }

    public class SockMgr
    {
        public delegate void SocketShutdownEventHandler(SockMgr source, SocketShutdownEventArgs e);
        public event SocketShutdownEventHandler SocketShutdownEvent;
        public delegate void SocketReceiveEventHandler(SockMgr source, SocketReceiveEventArgs e);
        public event SocketReceiveEventHandler SocketReceiveEvent;
        Socket _socket;
        public SocketRole Role { get; }
        Func<object, byte[]> _SerializeMethod;
        // public BufferMgr _bufferMgr;
        private bool _isReceiveStart = false;  // WORKAROUND

        public SockMgr(Socket s, SocketRole r)
        {
            _socket = s;
            this.Role = r;
            _SerializeMethod = null;
        }

        public Socket GetSocket()
        {
            return _socket;
        }

        public void SetSerializationMethod(Func<object, byte[]> serializeMethod)
        {
            _SerializeMethod = serializeMethod;
        }

        public void Send(object structuredData)
        {
            byte[] serializedData = _SerializeMethod(structuredData);

            int length = serializedData.Length;
            byte[] lengthByte = BitConverter.GetBytes(length);  // 4 Bytes
            _socket.Send(lengthByte);  // send prefix
            _socket.Send(serializedData);  // send data
        }

        public void StartReceive()
        {
            if (!_isReceiveStart)
            {
                _isReceiveStart = true;
                StateObject state = new StateObject();
                state.workSocket = _socket;
                state.Source = this;
                _socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None,
                    new AsyncCallback(ReadCallback), state);
            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
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
                    // add to buffer manager
                    state.bufferMgr.AddBytes(state.buffer, bytesRead);
                    // raise event
                    state.Source.SocketReceiveEvent?.Invoke(state.Source, new SocketReceiveEventArgs(state.bufferMgr));
                }

                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None,
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
            try
            {
                if (this.Role == SocketRole.Client)
                    _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (Exception)
            {

            }

            SocketShutdownEvent?.Invoke(this, new SocketShutdownEventArgs(true));
        }
    }
}
