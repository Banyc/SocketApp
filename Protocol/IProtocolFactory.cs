namespace SocketApp.Protocol
{
    public interface IProtocolFactory
    {
        void SetSockMgr(SockMgr sockMgr);
        ProtocolStack GetProtocolStack();
        IProtocolFactory Clone();
    }
}
