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
        SockList _sockList { set; get; } = new SockList();

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
            if (sockMgr.GetSockBase().Role == SocketRole.Listener)
                _sockList.Listeners.Remove(sockMgr);
            else
                _sockList.Clients.Remove(sockMgr);
        }

        // shutdown all listeners and clients
        public void ShutdownAll()
        {
            while (_sockList.Clients.Count > 0)
            {
                _sockList.Clients[0].Shutdown();
            }
            while (_sockList.Listeners.Count > 0)
            {
                _sockList.Listeners[0].Shutdown();
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

        // Tips: Set global behavior here
    }
}
