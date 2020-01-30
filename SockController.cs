using System.Threading;
using System.Collections.Generic;

namespace SocketApp
{
    public class SockList
    {
        public List<SockMgr> Clients { set; get; } = new List<SockMgr>();
        public List<SockMgr> Listeners { set; get; } = new List<SockMgr>();
    }

    // mother of all sockets under one process
    public class SockController
    {
        public delegate void SockMgrAcceptEventHandler(object sender, SockMgrAcceptEventArgs e);
        public event SockMgrAcceptEventHandler SockMgrAcceptEvent;
        public delegate void SockMgrConnectEventHandler(object sender, SockMgrConnectEventArgs e);
        public event SockMgrConnectEventHandler SockMgrConnectEvent;
        public delegate void SockMgrShutdownBeginEventHandler(object sender, SockMgrShutdownBeginEventArgs e);
        public event SockMgrShutdownBeginEventHandler SockMgrShutdownBeginEvent;
        public delegate void SockMgrReceiveEventHandler(object sender, SockMgrReceiveEventArgs e);
        public event SockMgrReceiveEventHandler SockMgrReceiveEvent;
        SockList _sockList { set; get; } = new SockList();
        SockFactory _sockFactory;
        Mutex _shutdownLock = new Mutex();  // eliminate race condition in `_sockList`

        public SockController()
        {
            _sockFactory = new SockFactory(this);
            // adapt events from _sockFactory
            _sockFactory.SockMgrAcceptEvent += OnSockMgrAccept;
            _sockFactory.SockMgrConnectEvent += OnSockMgrConnect;
        }

        public void BeginBuildTcp(SockFactoryOptions options, SocketRole socketRole)
        {
            _sockFactory.SetOptions(options);
            switch (socketRole)
            {
                case SocketRole.Client:
                    if (options.TimesToTry > 0)
                    {
                        _sockFactory.BuildTcpClient();
                    }
                    break;
                case SocketRole.Listener:
                    SockMgr listenerMgr = _sockFactory.GetTcpListener();
                    _sockFactory.ServerAccept(listenerMgr);
                    AddSockMgr(listenerMgr, SocketRole.Listener);
                    break;
            }
        }

        // event handlers
        private void OnSockMgrAccept(object sender, SockMgrAcceptEventArgs e)
        {
            RegisterNewSockMgr(e.Handler);
            SockMgrAcceptEvent?.Invoke(sender, e);
        }
        private void OnSockMgrConnect(object sender, SockMgrConnectEventArgs e)
        {
            RegisterNewSockMgr(e.Handler);
            SockMgrConnectEvent?.Invoke(sender, e);
        }
        private void OnSockMgrShutdownBegin(object sender, SockMgrShutdownBeginEventArgs e)
        {
            SockMgrShutdownBeginEvent?.Invoke(sender, e);
        }
        private void OnSockMgrReceive(object sender, SockMgrReceiveEventArgs e)
        {
            SockMgrReceiveEvent?.Invoke(sender, e);
        }
        // adapt events from new SockMgr
        private void RegisterNewSockMgr(SockMgr sockMgr)
        {
            sockMgr.SockMgrReceiveEvent += OnSockMgrReceive;
            sockMgr.SockMgrShutdownBeginEvent += OnSockMgrShutdownBegin;
        }

        // add sockMgr to sockList
        public void AddSockMgr(SockMgr sockMgr, SocketRole socketRole)
        {
            switch (socketRole)
            {
                case SocketRole.Client:
                    _sockList.Clients.Add(sockMgr);
                    break;
                case SocketRole.Listener:
                    _sockList.Listeners.Add(sockMgr);
                    break;
            }
        }

        public void RemoveSockMgr(SockMgr sockMgr)
        {
            _shutdownLock.WaitOne();
            if (sockMgr.GetSockBase().Role == SocketRole.Listener)
                _sockList.Listeners.Remove(sockMgr);
            else
                _sockList.Clients.Remove(sockMgr);
            _shutdownLock.ReleaseMutex();
        }

        // shutdown all listeners and clients
        public void ShutdownAll()
        {
            _shutdownLock.WaitOne();
            while (_sockList.Clients.Count > 0)
            {
                _shutdownLock.ReleaseMutex();
                _sockList.Clients[0].Shutdown();
                _shutdownLock.WaitOne();
            }
            while (_sockList.Listeners.Count > 0)
            {
                _shutdownLock.ReleaseMutex();
                _sockList.Listeners[0].Shutdown();
                _shutdownLock.WaitOne();
            }
        }

        public void ResetLists()
        {
            _sockList = new SockList();
        }
        public void SetLists(SockList sockList)
        {
            _sockList = sockList;
        }
        public SockList GetSockList()
        {
            return _sockList;
        }
        public Mutex GetShutdownLock()
        {
            return _shutdownLock;
        }

        // Tips: Set global behavior to sockets here
    }
}
