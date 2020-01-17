namespace SocketApp.Protocol
{
    public interface IProtocol<TypeOfLowLayer, TypeOfHighLayer, StateObject>
    {
        void SetState(StateObject state);
        StateObject GetState();
        TypeOfLowLayer GetDown(TypeOfHighLayer arg);
        TypeOfHighLayer GoUp(TypeOfLowLayer arg);
    }
}
