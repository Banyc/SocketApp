namespace SocketApp.Protocol
{
    public class ProtocolFactoryOptions
    {
        public bool EnableAes = false;
        public bool EnableRsa = false;
        public byte[] AesKey;
        public byte[] RsaPriKey;
        public byte[] RsaPubKey;
        public ProtocolStackType TextStackTypeOfChoice = ProtocolStackType.Text_Default;
    }

    public class ProtocolFactory
    {
        private SockController _sockController;
        private SockMgr _sockMgr;
        private ProtocolFactoryOptions _options = new ProtocolFactoryOptions();
        public ProtocolFactory(SockController sockController, SockMgr sockMgr, ProtocolFactoryOptions options)
        {
            _sockController = sockController;
            _sockMgr = sockMgr;
            _options = options;
        }
        public ProtocolFactory(SockController sockController, SockMgr sockMgr)
        {
            _sockController = sockController;
            _sockMgr = sockMgr;
        }

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
            
            switch (_options.TextStackTypeOfChoice)
            {
                case ProtocolStackType.Text_Broadcast:
                    protocolList.Text = GetBroadcastStack();
                    break;
                case ProtocolStackType.Text_Default:
                    protocolList.Text = GetDefaultStack();
                    break;
                default:
                    protocolList.Text = GetDefaultStack();
                    break;
            }
            return protocolList;
        }

        private ProtocolStack GetDefaultStack()
        {
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
            ProtocolStack protocolStack = new ProtocolStack();
            protocolStack.SetState(state);
            return protocolStack;
        }

        private ProtocolStack GetBroadcastStack()
        {
            ProtocolStackState state = new ProtocolStackState();

            BroadcastProtocolState broadcaseState = new BroadcastProtocolState();

            // Config for UTF8 layer

            // Config for AES layer
            AESProtocolState aesState = new AESProtocolState();
            if (_options.EnableAes)
                aesState.Key = _options.AesKey;

            broadcaseState.AesState = aesState;
            broadcaseState.SockController = _sockController;
            broadcaseState.SockMgr = _sockMgr;
            // add to stack
            state.MiddleProtocols.Add(new BroadcastProtocol(broadcaseState));

            state.Type = DataProtocolType.Text;
            ProtocolStack protocolStack = new ProtocolStack();
            protocolStack.SetState(state);
            return protocolStack;
        }
    }
}
