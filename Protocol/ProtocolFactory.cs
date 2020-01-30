namespace SocketApp.Protocol
{
    public class ProtocolFactoryOptions
    {
        public bool EnableRsa = false;
        public byte[] RsaPriKey;
        public byte[] RsaPubKey;
        public ProtocolStackType TextStackTypeOfChoice = ProtocolStackType.Text_Default;
        public AESProtocolState AESProtocolState = new AESProtocolState();
        // TODO: add customized sub factory for customized protocol stack
        public bool IsCustom = false;
        public IProtocolFactory Factory = null;
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

        public ProtocolStackList GetProtocolStackList()
        {
            ProtocolStackList ProtocolStackList = new ProtocolStackList();

            switch (_options.TextStackTypeOfChoice)
            {
                case ProtocolStackType.Text_Broadcast:
                    ProtocolStackList.Text = GetBroadcastStack();
                    break;
                case ProtocolStackType.Text_Default:
                    ProtocolStackList.Text = GetDefaultStack();
                    break;
                default:
                    ProtocolStackList.Text = GetDefaultStack();
                    break;
            }
            return ProtocolStackList;
        }

        private ProtocolStack GetDefaultStack()
        {
            ProtocolStackState state = new ProtocolStackState();

            // UTF8
            state.MiddleProtocols.Add(new UTF8Protocol());
            // AES
            AESProtocol aesP = new AESProtocol();
            AESProtocolState aesState = new AESProtocolState();
            aesP.SetState(_options.AESProtocolState);
            state.MiddleProtocols.Add(aesP);

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
            broadcaseState.AesState = _options.AESProtocolState;
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
