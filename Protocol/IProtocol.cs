namespace SocketApp.Protocol
{
    public interface IProtocol
    {
        void SetState(object state);
        object GetState();
        object GetDown(object arg);
        object GoUp(object arg);
    }
}
