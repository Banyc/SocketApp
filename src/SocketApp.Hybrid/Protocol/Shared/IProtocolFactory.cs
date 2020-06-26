using System;
namespace SocketApp.Protocol
{
    public interface IProtocolFactory : ICloneable
    {
        void SetSockMgr(SockMgr sockMgr);
        ProtocolStack GetProtocolStack();
    }
}
