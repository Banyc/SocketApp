using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketApp
{
    public class AcceptEventArgs
    {
        public AcceptEventArgs(SockMgr handler) { Handler = handler; }
        public SockMgr Handler { get; }
    }

    public class SockFactory
    {
        public delegate void AcceptEventHandler(SockFactory sender, AcceptEventArgs e);
        public event AcceptEventHandler AcceptEvent;
        IPAddress _ipAddress;
        int _listenerPort = 11000;
        int _localPort = -1;  // not for listener

        public void SetConfig(string ipAddress, int remotePort, int localPort = -1)
        {
            _ipAddress = IPAddress.Parse(ipAddress);
            _listenerPort = remotePort;
            _localPort = localPort;
        }

        public SockMgr GetTcpListener()
        {
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _listenerPort);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // makes restarting a socket become possible
            // https://blog.csdn.net/limlimlim/article/details/23424855
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            listener.Bind(localEndPoint);
            listener.Listen(4);

            SockMgr sockMgr = new SockMgr(listener, SocketRole.Listener);
            return sockMgr;
        }

        public void ServerAccept(SockMgr listener)
        {
            listener.GetSocket().BeginAccept(
                new System.AsyncCallback(AcceptCallback), listener.GetSocket());
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                SockMgr sockMgr = new SockMgr(handler, SocketRole.Client);
                BindingSockMgr(sockMgr);

                AcceptEvent?.Invoke(this, new AcceptEventArgs(sockMgr));

                listener.BeginAccept(
                    new System.AsyncCallback(AcceptCallback), listener);
            }
            catch (ObjectDisposedException) { }  // listener closed
        }

        public SockMgr GetTcpClient()
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            if (_localPort >= 0)
                sock.Bind(new IPEndPoint(IPAddress.Any, _localPort));

            sock.Connect(new IPEndPoint(_ipAddress, _listenerPort));

            SockMgr sockMgr = new SockMgr(sock, SocketRole.Client);
            BindingSockMgr(sockMgr);

            return sockMgr;
        }

        // <https://gist.github.com/louis-e/888d5031190408775ad130dde353e0fd>
        public SockMgr GetUdpListener()
        {
            Socket listener = new Socket(_ipAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            listener.Bind(new IPEndPoint(_ipAddress, _listenerPort));

            SockMgr sockMgr = new SockMgr(listener, SocketRole.Listener);
            BindingSockMgr(sockMgr);

            return sockMgr;
        }

        public SockMgr GetUdpClient()
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            sock.Connect(new IPEndPoint(_ipAddress, _listenerPort));

            SockMgr sockMgr = new SockMgr(sock, SocketRole.Client);
            BindingSockMgr(sockMgr);

            return sockMgr;
        }

        private void BindingSockMgr(SockMgr sockMgr)
        {
            sockMgr.SetSerializationMethod(Serialize);
            Responser responser = new Responser();
            sockMgr.SocketReceiveEvent += responser.OnSocketReceive;
            sockMgr.StartReceive();
        }

        static byte[] Serialize(object s)
        {
            return Encoding.UTF8.GetBytes((string)s);
        }
    }
}
