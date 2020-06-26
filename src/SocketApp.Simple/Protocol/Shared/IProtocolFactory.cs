using System;
namespace SocketApp.Simple.Protocol
{
    public interface IProtocolFactory : ICloneable
    {
        void SetSockMgr(SockMgr sockMgr);
        ProtocolStack GetProtocolStack();
    }
}
