namespace SocketApp.Protocol
{
    public class ProtocolFactoryOptions
    {
        public bool EnableRsa = false;
        public byte[] RsaPriKey;
        public byte[] RsaPubKey;
        public ProtocolStackType StackTypeOfChoice = ProtocolStackType.Default;
        public AESProtocolState AESProtocolState = new AESProtocolState();
        // TODO: add customized sub factory for customized protocol stack
        public bool IsCustom = false;  // if true, the Factory below will be used
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

        public ProtocolStack GetProtocolStack()
        {
            ProtocolStack ProtocolStack;

            if (_options.IsCustom)
            {
                return _options.Factory.GetProtocolStack();
            }

            switch (_options.StackTypeOfChoice)
            {
                case ProtocolStackType.Broadcast:
                    ProtocolStack = GetBroadcastStack();
                    break;
                case ProtocolStackType.Default:
                    ProtocolStack = GetDefaultStack();
                    break;
                default:
                    ProtocolStack = GetDefaultStack();
                    break;
            }
            return ProtocolStack;
        }

        private ProtocolStack GetDefaultStack()
        {
            ProtocolStackState state = new ProtocolStackState();

            // UTF8
            state.MiddleProtocols.Add(new UTF8Protocol());
            // AES
            AESProtocol aesP = new AESProtocol();
            aesP.SetState(_options.AESProtocolState.Clone());
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
            broadcaseState.AesState = _options.AESProtocolState.Clone();
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
