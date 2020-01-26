using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using SocketApp.Protocol;

namespace SocketApp
{
    // build SockMgr and connect it to Responser
    public class SockFactory
    {
        public event SockMgr.SockMgrAcceptEventHandler SockMgrAcceptEvent;
        public event SockMgr.SockMgrConnectEventHandler SockMgrConnectEvent;
        private SockController _sockController;
        IPAddress _ipAddress;
        int _listenerPort = 11000;
        int _localPort = -1;  // not for listener
        ProtocolFactoryOptions _protocolOptions = new ProtocolFactoryOptions();

        public SockFactory(SockController controller)
        {
            _sockController = controller;
        }

        public void SetConfig(string ipAddress, int remotePort, int localPort = -1)  // TODO: add Protocol
        {
            _ipAddress = IPAddress.Parse(ipAddress);
            _listenerPort = remotePort;
            _localPort = localPort;
        }

        public void SetProtocolOptions(Protocol.ProtocolFactoryOptions options)
        {
            _protocolOptions = options;
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

            SockBase sockBase = new SockBase(listener, SocketRole.Listener, true);
            // set config to `protocolFactory`
            ProtocolFactory protocolFactory = new ProtocolFactory(_protocolOptions);
            SockMgr sockMgr = new SockMgr(sockBase, _sockController, protocolFactory);

            listener.Bind(localEndPoint);
            listener.Listen(4);

            return sockMgr;
        }

        // start accepting
        public void ServerAccept(SockMgr listener)
        {
            listener.SockMgrAcceptEvent += OnSocketAccept;
            listener.GetSockBase().StartAccept();
        }
        // return
        private void OnSocketAccept(object sender, SockMgrAcceptEventArgs e)
        {
            SockMgrAcceptEvent?.Invoke(sender, e);
        }

        public void BuildTcpClient(int timesToTry)
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            if (_localPort >= 0)
                sock.Bind(new IPEndPoint(IPAddress.Any, _localPort));

            SockBase sockBase = new SockBase(sock, SocketRole.Client, false);
            // set config to `protocolFactory`
            ProtocolFactory protocolFactory = new ProtocolFactory(_protocolOptions);
            SockMgr sockMgr = new SockMgr(sockBase, _sockController, protocolFactory);

            sockMgr.GetSockBase().StartConnect(new IPEndPoint(_ipAddress, _listenerPort), timesToTry);
        }
        // return
        private void OnSocketConnect(object sender, SockMgrConnectEventArgs e)
        {
            SockMgrConnectEvent?.Invoke(sender, e);
        }

        // <https://gist.github.com/louis-e/888d5031190408775ad130dde353e0fd>
        public SockMgr GetUdpListener()
        {
            Socket listener = new Socket(_ipAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            listener.Bind(new IPEndPoint(_ipAddress, _listenerPort));

            SockBase sockBase = new SockBase(listener, SocketRole.Listener, true);
            // set config to `protocolFactory`
            ProtocolFactory protocolFactory = new ProtocolFactory(_protocolOptions);
            SockMgr sockMgr = new SockMgr(sockBase, _sockController, protocolFactory);

            return sockMgr;
        }

        public SockMgr GetUdpClient()
        {
            Socket sock = new Socket(_ipAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            SockBase sockBase = new SockBase(sock, SocketRole.Client, false);
            // set config to `protocolFactory`
            ProtocolFactory protocolFactory = new ProtocolFactory(_protocolOptions);
            SockMgr sockMgr = new SockMgr(sockBase, _sockController, protocolFactory);

            // TODO: use BeginConnect instead
            sock.Connect(new IPEndPoint(_ipAddress, _listenerPort));

            return sockMgr;
        }
    }
}
