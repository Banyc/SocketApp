namespace SocketApp
{
    public class SockMgrEventArgs
    {
        public SockMgr Handler;
    }
    public class SockMgrAcceptEventArgs : SockMgrEventArgs
    {
        public SockMgrAcceptEventArgs(SockMgr handler, AcceptStateObject state, object externalCallbackState = null) { State = state; ExternalCallbackState = externalCallbackState; base.Handler = handler; }
        public AcceptStateObject State { get; }
        public object ExternalCallbackState  { get; }
    }
    public class SockMgrConnectEventArgs : SockMgrEventArgs
    {
        public SockMgrConnectEventArgs(SockMgr handler, ConnectStateObject state, object externalCallbackState = null) { State = state; ExternalCallbackState = externalCallbackState; base.Handler = handler; }
        public ConnectStateObject State { get; }
        public object ExternalCallbackState  { get; }
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

    public class SockMgrConnectStateObject
    {
        public SockMgr.SockMgrConnectEventHandler externalCallback = null;
        public object externalCallbackState = null;
    }

    public class SockMgrAcceptStateObject
    {
        public SockMgr.SockMgrAcceptEventHandler externalCallback = null;
        public object externalCallbackState = null;
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
        Protocol.ProtocolStack _protocolStack;
        Protocol.IProtocolFactory _protocolFactory;
        SockBase _sockBase;
        public bool IsShutdown = false;

        public SockMgr(SockBase sockBase, SockController sockController, Protocol.IProtocolFactory protocolFactory)
        {
            _sockBase = sockBase;
            _sockBase.SocketReceiveEvent += OnSocketReceive;
            _sockBase.SocketShutdownBeginEvent += OnSocketShutdownBegin;
            _sockController = sockController;

            protocolFactory.SetSockMgr(this);  // TODO: review
            _protocolFactory = protocolFactory;

            _protocolStack = protocolFactory.GetProtocolStack();

            Responser responser = new Responser(_sockController, _protocolStack, this);
            _responser = responser;
        }

        public void StartConnect(System.Net.IPEndPoint ep, int timesToTry, SockMgrConnectEventHandler externalCallback = null, object externalCallbackState = null)
        {
            SockMgrConnectStateObject state = new SockMgrConnectStateObject();
            state.externalCallback = externalCallback;
            state.externalCallbackState = externalCallbackState;
            _sockBase.StartConnect(ep, timesToTry, ConnectCallback, state);
        }
        private void ConnectCallback(object sender, SocketConnectEventArgs e)
        {
            SockMgrConnectStateObject state = (SockMgrConnectStateObject)e.State.externalCallbackState;
            SockMgrConnectEventArgs arg = new SockMgrConnectEventArgs(this, e.State, state.externalCallbackState);
            SockMgrConnectEvent?.Invoke(this, arg);
            if (state.externalCallback != null)
                state.externalCallback(this, arg);
            _responser.OnSockMgrConnect(this, arg);
        }

        public void StartAccept(SockMgrAcceptEventHandler externalCallback = null, object externalCallbackState = null)
        {
            SockMgrAcceptStateObject state = new SockMgrAcceptStateObject();
            state.externalCallback = externalCallback;
            state.externalCallbackState = externalCallbackState;
            _sockBase.StartAccept(AcceptCallback, state);
        }
        private void AcceptCallback(object sender, SocketAcceptEventArgs e)
        {
            SockMgrAcceptStateObject state = (SockMgrAcceptStateObject)e.State.externalCallbackState;
            SockMgr client = new SockMgr(e.Handler, _sockController, _protocolFactory.Clone());
            SockMgrAcceptEventArgs arg = new SockMgrAcceptEventArgs(client, e.State, state.externalCallbackState);
            SockMgrAcceptEvent?.Invoke(this, arg);
            if (state.externalCallback != null)
                state.externalCallback(this, arg);
            _responser.OnSockMgrAccept(this, arg);
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
        public void SetProtocolStack(Protocol.ProtocolStack protocolStack)
        {
            _protocolStack = protocolStack;
            _responser.LinkProtocolStackEvents(protocolStack);
        }
        public Protocol.ProtocolStack GetProtocolStack()
        {
            return _protocolStack;
        }
        public SockBase GetSockBase()
        {
            return _sockBase;
        }

        public void SendText(string data)
        {
            Protocol.DataContent dataContent = new Protocol.DataContent();
            dataContent.Type = Protocol.DataProtocolType.Text;
            dataContent.Data = data;
            _protocolStack.FromHighLayerToHere(dataContent);
        }
        public void SendFile(byte[] data)
        {
            Protocol.DataContent dataContent = new Protocol.DataContent();
            dataContent.Type = Protocol.DataProtocolType.File;
            dataContent.Data = data;
            _protocolStack.FromHighLayerToHere(dataContent);
        }
        
        // dataContent has been processed and delivered to the topest layer of Application
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
