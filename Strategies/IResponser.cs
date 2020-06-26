using SocketApp.Protocol;

namespace SocketApp
{
    public interface IResponser
    {
        void LinkProtocolStackEvents(ProtocolStack protocolStack);
        void UnlinkProtocolStack(ProtocolStack protocolStack);
        void OnSockMgrReceive(object sender, SockMgrReceiveEventArgs e);
        void OnSockMgrConnect(object sender, SockMgrConnectEventArgs e);
        void OnSockMgrAccept(object sender, SockMgrAcceptEventArgs e);
        void OnSockMgrShutdownBegin(object sender, SockMgrShutdownBeginEventArgs e);
    }
}
