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

    public class SockMgrProtocolTopEventArgs : SockMgrEventArgs
    {
        public SockMgrProtocolTopEventArgs(SockMgr handler, Protocol.DataContent dataContent) { DataContent = dataContent; base.Handler = handler; }
        public Protocol.DataContent DataContent;
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
        public delegate void SockMgrProtocolTopEventHandler(object sender, SockMgrProtocolTopEventArgs e);
        public event SockMgrProtocolTopEventHandler SockMgrProtocolTopEvent;

        Responser _responser;
        SockController _sockController;
        Protocol.ProtocolStackList _ProtocolStackList;
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
            _sockController = sockController;
            
            _protocolFactory = new Protocol.ProtocolFactory(_sockController, this, protocolOptions);
            _ProtocolStackList = _protocolFactory.GetProtocolStackList();

            Responser responser = new Responser(_sockController, _ProtocolStackList, this);
            _responser = responser;
        }

        private void OnSocketAccept(object sender, SocketAcceptEventArgs e)
        {
            // Notice: all clients derived from the same listener share the same factory,
            //  which means the modification on the shared factory will affect factory in other clients
            SockMgr client = new SockMgr(e.Handler, _sockController, _protocolFactory.GetOptions());
            SockMgrAcceptEventArgs arg = new SockMgrAcceptEventArgs(client);
            SockMgrAcceptEvent?.Invoke(this, arg);
            _responser.OnSockMgrAccept(this, arg);
        }
        private void OnSocketConnect(object sender, SocketConnectEventArgs e)
        {
            SockMgrConnectEventArgs arg = new SockMgrConnectEventArgs(this, e.State);
            SockMgrConnectEvent?.Invoke(this, arg);
            _responser.OnSockMgrConnect(this, arg);
        }
        private void OnSocketShutdownBegin(object sender, SocketShutdownBeginEventArgs e)
        {
            this.IsShutdown = true;
            SockMgrShutdownBeginEventArgs arg = new SockMgrShutdownBeginEventArgs(this, e.IsShutdown);
            SockMgrShutdownBeginEvent?.Invoke(this, arg);
            _responser.OnSockMgrShutdownBegin(this, arg);
        }
        private void OnSocketReceive(object sender, SocketReceiveEventArgs e)
        {
            SockMgrReceiveEventArgs arg = new SockMgrReceiveEventArgs(this, e.BufferMgr);
            SockMgrReceiveEvent?.Invoke(this, arg);
            _responser.OnSockMgrReceive(this, arg);
        }

        public Responser GetResponser()
        {
            return _responser;
        }
        public void SetProtocolStackList(Protocol.ProtocolStackList ProtocolStackList)
        {
            _ProtocolStackList = ProtocolStackList;
            _responser.SetProtocolStackList(_ProtocolStackList);
        }
        public Protocol.ProtocolStackList GetProtocolStackList()
        {
            return _ProtocolStackList;
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
            _ProtocolStackList.Text.FromHighLayerToHere(dataContent);
        }
        public void RaiseSockMgrProtocolTopEvent(Protocol.DataContent dataContent)
        {
            SockMgrProtocolTopEvent?.Invoke(this, new SockMgrProtocolTopEventArgs(this, dataContent));
        }

        public void Shutdown()
        {
            this.IsShutdown = true;
            _sockBase.Shutdown();
        }
    }
}
