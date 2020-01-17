using System.Collections.Generic;

namespace SocketApp
{
    public class SockList
    {
        public List<SockMgr> Clients = new List<SockMgr>();
        public List<SockMgr> Listeners = new List<SockMgr>();
    }
}
