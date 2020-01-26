namespace SocketApp
{
    public class SockMgrEventArgs
    {
        public SockMgr Handler;
    }
    public class SockMgrAcceptEventArgs : SockMgrEventArgs
    {
        public SockMgrAcceptEventArgs(SockMgr handler) { base.Handler = handler; }
    }
    public class SockMgrConnectEventArgs : SockMgrEventArgs
    {
        public SockMgrConnectEventArgs(SockMgr handler, ConnectStateObject state) { State = state; base.Handler = handler; }
        public ConnectStateObject State { get; }
    }

    public class SockMgrShutdownBeginEventArgs : SockMgrEventArgs
    {
        public SockMgrShutdownBeginEventArgs(SockMgr handler, bool isShutdown) { IsShutdown = isShutdown; base.Handler = handler; }
        public bool IsShutdown { get; }
    }

    public class SockMgrReceiveEventArgs : SockMgrEventArgs
    {
        public SockMgrReceiveEventArgs(SockMgr handler, BufferMgr bufferMgr) { BufferMgr = bufferMgr; base.Handler = handler; }
        public BufferMgr BufferMgr;
    }

    // a wrapper of `SockBase` and the corresponding `Responser`
    public class SockMgr
    {
        public delegate void SockMgrAcceptEventHandler(object sender, SockMgrAcceptEventArgs e);
        public event SockMgrAcceptEventHandler SockMgrAcceptEvent;
        public delegate void SockMgrConnectEventHandler(object sender, SockMgrConnectEventArgs e);
        public event SockMgrConnectEventHandler SockMgrConnectEvent;
        public delegate void SockMgrShutdownBeginEventHandler(object sender, SockMgrShutdownBeginEventArgs e);
        public event SockMgrShutdownBeginEventHandler SockMgrShutdownBeginEvent;
        public delegate void SockMgrReceiveEventHandler(object sender, SockMgrReceiveEventArgs e);
        public event SockMgrReceiveEventHandler SockMgrReceiveEvent;

        Responser _responser;
        SockController _sockController;
        Protocol.ProtocolList _protocolList;
        Protocol.ProtocolFactory _protocolFactory;
        SockBase _sockBase;
        public bool IsShutdown = false;

        public SockMgr(SockBase sockBase, SockController sockController, Protocol.ProtocolFactoryOptions protocolOptions)
        {
            _sockBase = sockBase;
            _sockBase.SocketAcceptEvent += OnSocketAccept;
            _sockBase.SocketConnectEvent += OnSocketConnect;
            _sockBase.SocketReceiveEvent += OnSocketReceive;
            _sockBase.SocketShutdownBeginEvent += OnSocketShutdownBegin;
            _protocolFactory = new Protocol.ProtocolFactory(_sockController, this, protocolOptions);
            _protocolList = _protocolFactory.GetProtocolList();
            _sockController = sockController;

            Responser responser = new Responser(_sockController, _protocolList, this);
            _responser = responser;
        }

        private void OnSocketAccept(object sender, SocketAcceptEventArgs e)
        {
            // Notice: all clients derived from the same listener share the same factory,
            //  which means the modification on the shared factory will affect factory in other clients
            SockMgr client = new SockMgr(e.Handler, _sockController, _protocolFactory.GetOptions());
            SockMgrAcceptEventArgs arg = new SockMgrAcceptEventArgs(client);
            _responser.OnSockMgrAccept(this, arg);
            SockMgrAcceptEvent?.Invoke(this, arg);
        }
        private void OnSocketConnect(object sender, SocketConnectEventArgs e)
        {
            SockMgrConnectEventArgs arg = new SockMgrConnectEventArgs(this, e.State);
            _responser.OnSockMgrConnect(this, arg);
            SockMgrConnectEvent?.Invoke(this, arg);
        }
        private void OnSocketShutdownBegin(object sender, SocketShutdownBeginEventArgs e)
        {
            SockMgrShutdownBeginEventArgs arg = new SockMgrShutdownBeginEventArgs(this, e.IsShutdown);
            _responser.OnSockMgrShutdownBegin(this, arg);
            SockMgrShutdownBeginEvent?.Invoke(this, arg);
        }
        private void OnSocketReceive(object sender, SocketReceiveEventArgs e)
        {
            SockMgrReceiveEventArgs arg = new SockMgrReceiveEventArgs(this, e.BufferMgr);
            _responser.OnSockMgrReceive(this, arg);
            SockMgrReceiveEvent?.Invoke(this, arg);
        }
        public Responser GetResponser()
        {
            return _responser;
        }
        public void SetProtocolList(Protocol.ProtocolList protocolList)
        {
            _protocolList = protocolList;
            _responser.SetProtocolList(_protocolList);
        }
        public SockBase GetSockBase()
        {
            return _sockBase;
        }

        public void SendText(string data)  // TODO: test
        {
            Protocol.DataContent dataContent = new Protocol.DataContent();
            dataContent.Type = Protocol.DataProtocolType.Text;
            dataContent.Data = data;
            _protocolList.Text.FromHighLayerToHere(dataContent);
        }

        public void Shutdown()
        {
            this.IsShutdown = true;
            _sockBase.Shutdown();
        }
    }
}
