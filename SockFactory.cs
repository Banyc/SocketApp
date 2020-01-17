using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace SocketApp
{
    // build SockMgr and connect it to Responser
    public class SockFactory
    {
        public event SockMgr.SocketAcceptEventHandler SocketAcceptEvent;
        public event SockMgr.SocketConnectEventHandler SocketConnectEvent;
        IPAddress _ipAddress;
        int _listenerPort = 11000;
        int _localPort = -1;  // not for listener
        List<SockMgr> _clients = new List<SockMgr>();
        List<SockMgr> _listeners = new List<SockMgr>();

        public void ResetLists()
        {
            _clients = new List<SockMgr>();
            _listeners = new List<SockMgr>();
        }
        public void SetLists(List<SockMgr> clients, List<SockMgr> listeners)
        {
            _clients = clients;
            _listeners = listeners;
        }
        public List<SockMgr> GetClientList()
        {
            return _clients;
        }
        public List<SockMgr> GetListenerList()
        {
            return _listeners;
        }
        public void SetConfig(string ipAddress, int remotePort, int localPort = -1)  // TODO: add Protocol
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
            
            SockMgr sockMgr = new SockMgr(listener, SocketRole.Listener, true);
            InitSockMgr(sockMgr);

            listener.Bind(localEndPoint);
            listener.Listen(4);

            return sockMgr;
        }

        // start accepting
        public void ServerAccept(SockMgr listener)
        {
            InitSockMgr(listener);
            listener.SocketAcceptEvent += OnSocketAccept;
            listener.StartAccept();
        }
        // return
        private void OnSocketAccept(SockMgr sender, SocketAcceptEventArgs e)
        {
            Responser responser = InitSockMgr(e.Handler);
            responser.OnSocketAccept(sender, e);
            SocketAcceptEvent?.Invoke(sender, e);
        }

        public void BuildTcpClient(int timesToTry)
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            if (_localPort >= 0)
                sock.Bind(new IPEndPoint(IPAddress.Any, _localPort));

            SockMgr sockMgr = new SockMgr(sock, SocketRole.Client, false);
            InitSockMgr(sockMgr);
            sockMgr.StartConnect(new IPEndPoint(_ipAddress, _listenerPort), timesToTry);
        }
        // return
        private void OnSocketConnect(object sender, SocketConnectEventArgs e)
        {
            SocketConnectEvent?.Invoke(sender, e);
        }

        // <https://gist.github.com/louis-e/888d5031190408775ad130dde353e0fd>
        public SockMgr GetUdpListener()
        {
            Socket listener = new Socket(_ipAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            listener.Bind(new IPEndPoint(_ipAddress, _listenerPort));

            SockMgr sockMgr = new SockMgr(listener, SocketRole.Listener, true);
            InitSockMgr(sockMgr);

            return sockMgr;
        }

        public SockMgr GetUdpClient()
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            SockMgr sockMgr = new SockMgr(sock, SocketRole.Client, false);
            InitSockMgr(sockMgr);

            // TODO: use BeginConnect instead
            sock.Connect(new IPEndPoint(_ipAddress, _listenerPort));

            return sockMgr;
        }

        // init sockMgr
        private Responser InitSockMgr(SockMgr sockMgr)
        {
            if (sockMgr.Role == SocketRole.Client)
                sockMgr.SetSerializationMethod(Serialize);
            Responser responser = new Responser(_clients, _listeners);
            if (sockMgr.Role == SocketRole.Client)
                sockMgr.SocketConnectEvent += responser.OnSocketConnect;
            sockMgr.SocketShutdownBeginEvent += responser.OnSocketShutdownBegin;
            if (sockMgr.Role == SocketRole.Client)
            {
                sockMgr.SocketReceiveEvent += responser.OnSocketReceive;
            }
            return responser;
        }

        static byte[] Serialize(object s)
        {
            return Encoding.UTF8.GetBytes((string)s);
        }
    }
}
