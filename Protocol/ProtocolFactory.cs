namespace SocketApp.Protocol
{
    public class ProtocolFactoryOptions
    {
        public bool EnableAes = false;
        public bool EnableRsa = false;
        public byte[] AesKey;
        public byte[] RsaPriKey;
        public byte[] RsaPubKey;
    }

    public class ProtocolFactory
    {
        private ProtocolFactoryOptions _options = new ProtocolFactoryOptions();
        public ProtocolFactory(ProtocolFactoryOptions options)
        {
            _options = options;
        }
        public ProtocolFactory() { }

        public void SetOptions(ProtocolFactoryOptions options)
        {
            _options = options;
        }

        public ProtocolFactoryOptions GetOptions()
        {
            return _options;
        }

        public ProtocolList GetProtocolList()
        {
            ProtocolList protocolList = new ProtocolList();
            ProtocolStackState state = new ProtocolStackState();

            // UTF8
            state.MiddleProtocols.Add(new UTF8Protocol());
            // AES
            if (_options.EnableAes)
            {
                AESProtocol aesP = new AESProtocol();
                AESProtocolState aesState = new AESProtocolState();
                aesState.Key = _options.AesKey;
                aesP.SetState(aesState);
                state.MiddleProtocols.Add(aesP);
            }

            state.Type = DataProtocolType.Text;
            ProtocolStack ProtocolStack = new ProtocolStack();
            ProtocolStack.SetState(state);
            protocolList.Text = ProtocolStack;
            return protocolList;
        }
    }
}
