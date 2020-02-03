using System.Collections.Generic;

namespace SocketApp.Protocol
{
    public class DefaultProtocolFactoryOptions
    {
        public SockController SockController;
        public SockMgr SockMgr;
        public bool EnableRsa = false;
        public byte[] RsaPriKey;
        public byte[] RsaPubKey;
        public ProtocolStackType StackTypeOfChoice = ProtocolStackType.Default;
        public AESProtocolState AESProtocolState = new AESProtocolState();

        public DefaultProtocolFactoryOptions Clone()
        {
            DefaultProtocolFactoryOptions options = new DefaultProtocolFactoryOptions();
            options.EnableRsa = this.EnableRsa;
            options.RsaPriKey = (byte[])this.RsaPriKey?.Clone();
            options.RsaPubKey = (byte[])this.RsaPubKey?.Clone();
            options.StackTypeOfChoice = this.StackTypeOfChoice;  // test
            options.AESProtocolState = this.AESProtocolState.Clone();
            options.SockController = this.SockController;
            options.SockMgr = this.SockMgr;
            return options;
        }
    }

    // to make it a static class?
    public class DefaultProtocolFactory : IProtocolFactory
    {
        private DefaultProtocolFactoryOptions _options = new DefaultProtocolFactoryOptions();
        public DefaultProtocolFactory(DefaultProtocolFactoryOptions options)
        {
            _options = options;
        }
        public DefaultProtocolFactory()
        {
        }

        public void SetOptions(DefaultProtocolFactoryOptions options)
        {
            _options = options;
        }

        public DefaultProtocolFactoryOptions GetOptions()
        {
            return _options;
        }

        public ProtocolStack GetProtocolStack()
        {
            ProtocolStack ProtocolStack;

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

            // File Branch
            // TODO

            // Text Branch
            ProtocolStackState textBranchState = new ProtocolStackState();
            textBranchState.MiddleProtocols.Add(new UTF8Protocol());  // UTF8
            ProtocolStack textBranch = new ProtocolStack();
            textBranch.SetState(textBranchState);

            // branching
            List<ProtocolStack> branches = new List<ProtocolStack>();
            branches.Add(textBranch);
            TypeBranchingProtocol branchingProtocol = new TypeBranchingProtocol();
            branchingProtocol.SetBranches(branches);
            state.MiddleProtocols.Add(branchingProtocol);

            // Type tagging
            TypeTagProtocol typeTagProtocol = new TypeTagProtocol();
            state.MiddleProtocols.Add(typeTagProtocol);

            // Seq
            state.MiddleProtocols.Add(new SequenceProtocol());
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
            broadcaseState.SockController = _options.SockController;
            broadcaseState.SockMgr = _options.SockMgr;
            // add to stack
            state.MiddleProtocols.Add(new BroadcastProtocol(broadcaseState));

            state.Type = DataProtocolType.Text;
            ProtocolStack protocolStack = new ProtocolStack();
            protocolStack.SetState(state);
            return protocolStack;
        }

        public IProtocolFactory Clone()
        {
            DefaultProtocolFactory factory = new DefaultProtocolFactory(_options.Clone());
            return factory;
        }

        public void SetSockMgr(SockMgr sockMgr)
        {
            _options.SockMgr = sockMgr;
        }
    }
}
