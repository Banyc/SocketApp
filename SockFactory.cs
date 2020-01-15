using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

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
        public event SockMgr.SocketConnectEventHandler SocketConnectEvent;
        IPAddress _ipAddress;
        int _listenerPort = 11000;
        int _localPort = -1;  // not for listener
        List<SockMgr> _clients;
        List<SockMgr> _listeners;

        public void SetLists(List<SockMgr> clients, List<SockMgr> listeners)
        {
            _clients = clients;
            _listeners = listeners;
        }
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

            SockMgr sockMgr = new SockMgr(listener, SocketRole.Listener, true);
            return sockMgr;
        }

        public void ServerAccept(SockMgr listener)
        {
            listener.SocketAcceptEvent += OnSocketAccept;
            listener.StartAccept();
        }

        private void OnSocketAccept(SockMgr sender, SocketAcceptEventArgs e)
        {
            BindingSockMgr(e.Handler);
            AcceptEvent?.Invoke(this, new AcceptEventArgs(e.Handler));
        }

        public void BuildTcpClient(int timesToTry)
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            if (_localPort >= 0)
                sock.Bind(new IPEndPoint(IPAddress.Any, _localPort));

            SockMgr sockMgr = new SockMgr(sock, SocketRole.Client, false);
            sockMgr.SocketConnectEvent += OnSocketConnect;
            sockMgr.StartConnect(new IPEndPoint(_ipAddress, _listenerPort), timesToTry);
        }

        private void OnSocketConnect(object sender, SocketConnectEventArgs e)
        {
            if (!e.Handler.IsConnected)
            {
                SocketConnectEvent?.Invoke(this, e);
                return;
            }
            BindingSockMgr(e.Handler);
            SocketConnectEvent?.Invoke(this, e);
        }

        public SockMgr GetTcpClient()
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            if (_localPort >= 0)
                sock.Bind(new IPEndPoint(IPAddress.Any, _localPort));

            sock.Connect(new IPEndPoint(_ipAddress, _listenerPort));

            SockMgr sockMgr = new SockMgr(sock, SocketRole.Client, false);
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

            SockMgr sockMgr = new SockMgr(listener, SocketRole.Listener, true);
            BindingSockMgr(sockMgr);

            return sockMgr;
        }

        public SockMgr GetUdpClient()
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            sock.Connect(new IPEndPoint(_ipAddress, _listenerPort));

            SockMgr sockMgr = new SockMgr(sock, SocketRole.Client, false);
            BindingSockMgr(sockMgr);

            return sockMgr;
        }

        private void BindingSockMgr(SockMgr sockMgr)
        {
            sockMgr.SetSerializationMethod(Serialize);
            Responser responser = new Responser(_clients, _listeners);
            if (sockMgr.Role == SocketRole.Client)
                responser.OnSocketConnected(sockMgr);
            sockMgr.SocketShutdownBeginEvent += responser.OnSocketShutdownBegin;
            sockMgr.SocketReceiveEvent += responser.OnSocketReceive;
            sockMgr.StartReceive();
        }

        static byte[] Serialize(object s)
        {
            return Encoding.UTF8.GetBytes((string)s);
        }
    }
}
