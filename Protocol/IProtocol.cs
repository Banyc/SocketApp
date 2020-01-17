namespace SocketApp.Protocol
{
    public interface IProtocol<TypeOfHighLayer, TypeOfLowLayer, StateObject>
    {
        void SetState(StateObject state);
        StateObject GetState();
        TypeOfLowLayer GetDown(TypeOfHighLayer arg);
        TypeOfHighLayer GoUp(TypeOfLowLayer arg);
    }
}
