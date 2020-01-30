namespace SocketApp.Protocol
{
    public interface IProtocolFactory
    {
        ProtocolStack GetProtocolStack();
    }
}
