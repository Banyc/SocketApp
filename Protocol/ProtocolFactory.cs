namespace SocketApp.Protocol
{
    public class ProtocolFactory
    {
        public ProtocolFactory()
        {
        }

        // TODO: Set Options

        public ProtocolList GetProtocolList()
        {
            ProtocolList protocolList = new ProtocolList();
            ProtocolStackState state = new ProtocolStackState();
            state.MiddleProtocols.Add(new UTF8Protocol());
            state.Type = DataProtocolType.Text;
            ProtocolStack ProtocolStack = new ProtocolStack();
            ProtocolStack.SetState(state);
            protocolList.Text = ProtocolStack;
            return protocolList;
        }
    }
}
