using System.Net.Sockets;

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

    // socket manager to provide basic asynchronous interface of system socket
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
        SockList _sockList;
        Protocol.ProtocolList _protocolList;
        SockBase _sockBase;
        bool _isShutdown = false;

        public SockMgr(SockBase sockBase, SockList sockList, Protocol.ProtocolList protocolList)
        {
            _sockBase = sockBase;
            _sockBase.SocketAcceptEvent += OnSocketAccept;
            _sockBase.SocketConnectEvent += OnSocketConnect;
            _sockBase.SocketReceiveEvent += OnSocketReceive;
            _sockBase.SocketShutdownBeginEvent += OnSocketShutdownBegin;
            _protocolList = protocolList;
            _sockList = sockList;

            Responser responser = new Responser(sockList, _protocolList.Text.GoUp);
            _responser = responser;
        }

        private void OnSocketAccept(object sender, SocketAcceptEventArgs e)
        {
            SockMgr client = new SockMgr(e.Handler, _sockList, _protocolList);
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
            _responser.SetDeserializeMethod(_protocolList.Text.GoUp);
        }
        public SockBase GetSockBase()
        {
            return _sockBase;
        }
        public void Send(object data)
        {
            _sockBase.Send((byte[]) _protocolList.Text.GetDown(data));
        }

        public void Shutdown()
        {
            _isShutdown = true;
            _sockBase.Shutdown();
        }
    }
}
