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

            // UTF8
            state.MiddleProtocols.Add(new UTF8Protocol());
            // AES
            AESProtocol aesP = new AESProtocol();
            AESProtocolState aesState = new AESProtocolState();
            // WORKAROUND
            aesState.Key = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            aesP.SetState(aesState);
            state.MiddleProtocols.Add(aesP);

            state.Type = DataProtocolType.Text;
            ProtocolStack ProtocolStack = new ProtocolStack();
            ProtocolStack.SetState(state);
            protocolList.Text = ProtocolStack;
            return protocolList;
        }
    }
}
