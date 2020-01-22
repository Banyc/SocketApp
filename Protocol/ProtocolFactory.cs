namespace SocketApp.Protocol
{
    public class ProtocolFactory
    {
        public ProtocolFactory()
        {
        }

        public ProtocolList GetProtocolList()
        {
            ProtocolList protocolList = new ProtocolList();
            FullProtocolStacksState state = new FullProtocolStacksState();
            state.MiddleProtocols.Add(new UTF8Protocol());
            state.Type = DataProtocolType.Text;
            FullProtocolStacks fullProtocolStacks = new FullProtocolStacks();
            fullProtocolStacks.SetState(state);
            protocolList.Text = fullProtocolStacks;
            return protocolList;
        }
    }
}
