using System.Net;
using System.Net.Sockets;
using SocketApp.Simple.Protocol;

namespace SocketApp.Simple
{
    public class SockFactoryOptions
    {
        public IPAddress ListenerIpAddress;
        public int ListenerPort;
        public int ClientPort = -1;
        public int TimesToTry = 1;  // For client only
        public IProtocolFactory ProtocolFactory;  // TODO: new default factory
    }

    // build SockMgr and connect it to Responser
    public class SockFactory
    {
        public event SockMgr.SockMgrAcceptEventHandler SockMgrAcceptEvent;
        public event SockMgr.SockMgrConnectEventHandler SockMgrConnectEvent;
        SockFactoryOptions _options;

        public SockFactory()
        {
        }

        public void SetOptions(SockFactoryOptions options)
        {
            _options = options;
        }

        public SockMgr GetTcpListener()
        {
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _options.ListenerPort);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // makes restarting a socket become possible
            // https://blog.csdn.net/limlimlim/article/details/23424855
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            SockBase sockBase = new SockBase(listener, SocketRole.Listener, true);
            SockMgr sockMgr = new SockMgr(sockBase, (IProtocolFactory)_options.ProtocolFactory.Clone());

            listener.Bind(localEndPoint);
            listener.Listen(4);

            return sockMgr;
        }

        // start accepting
        public void ServerAccept(SockMgr listener, SockMgr.SockMgrAcceptEventHandler acceptCallback = null, object callbackState = null)
        {
            listener.SockMgrAcceptEvent += OnSocketAccept;
            listener.StartAccept(acceptCallback, callbackState);
        }
        // return
        private void OnSocketAccept(object sender, SockMgrAcceptEventArgs e)
        {
            SockMgrAcceptEvent?.Invoke(sender, e);
        }

        public void BuildTcpClient(SockMgr.SockMgrConnectEventHandler connectCallback, object callbackState = null)
        {
            Socket sock = new Socket(_options.ListenerIpAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            if (_options.ClientPort >= 0)
                sock.Bind(new IPEndPoint(IPAddress.Any, _options.ClientPort));

            SockBase sockBase = new SockBase(sock, SocketRole.Client, false);
            SockMgr sockMgr = new SockMgr(sockBase, (IProtocolFactory)_options.ProtocolFactory.Clone());

            sockMgr.SockMgrConnectEvent += OnSocketConnect;
            sockMgr.StartConnect(new IPEndPoint(_options.ListenerIpAddress, _options.ListenerPort), _options.TimesToTry, connectCallback, callbackState);
        }
        // return
        private void OnSocketConnect(object sender, SockMgrConnectEventArgs e)
        {
            SockMgrConnectEvent?.Invoke(sender, e);
        }

        // <https://gist.github.com/louis-e/888d5031190408775ad130dde353e0fd>
        public SockMgr GetUdpListener()
        {
            Socket listener = new Socket(_options.ListenerIpAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            listener.Bind(new IPEndPoint(_options.ListenerIpAddress, _options.ListenerPort));

            SockBase sockBase = new SockBase(listener, SocketRole.Listener, true);
            SockMgr sockMgr = new SockMgr(sockBase, (IProtocolFactory)_options.ProtocolFactory.Clone());

            return sockMgr;
        }

        public SockMgr GetUdpClient()
        {
            Socket sock = new Socket(_options.ListenerIpAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            SockBase sockBase = new SockBase(sock, SocketRole.Client, false);
            SockMgr sockMgr = new SockMgr(sockBase, (IProtocolFactory)_options.ProtocolFactory.Clone());

            // TODO: use BeginConnect instead
            sock.Connect(new IPEndPoint(_options.ListenerIpAddress, _options.ListenerPort));

            return sockMgr;
        }
    }
}
