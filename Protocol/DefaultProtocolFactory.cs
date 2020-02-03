using System;
using System.Collections.Generic;

namespace SocketApp.Protocol
{
    public class DefaultProtocolFactoryOptions : ICloneable
    {
        public SockController SockController;
        public SockMgr SockMgr;
        public bool EnableRsa = false;
        public byte[] RsaPriKey;
        public byte[] RsaPubKey;
        public ProtocolStackType StackTypeOfChoice = ProtocolStackType.Default;
        public AESProtocolState SecondLowAESProtocolState = new AESProtocolState();
        public AESProtocolState FirstLowAESProtocolState = new AESProtocolState();

        public object Clone()
        {
            DefaultProtocolFactoryOptions options = new DefaultProtocolFactoryOptions();
            options.EnableRsa = this.EnableRsa;
            options.RsaPriKey = (byte[])this.RsaPriKey?.Clone();
            options.RsaPubKey = (byte[])this.RsaPubKey?.Clone();
            options.StackTypeOfChoice = this.StackTypeOfChoice;  // test
            options.SecondLowAESProtocolState = (AESProtocolState)this.SecondLowAESProtocolState.Clone();
            options.FirstLowAESProtocolState = (AESProtocolState)this.FirstLowAESProtocolState.Clone();
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
            textBranchState.Type = DataProtocolType.Text;

            // branching
            List<ProtocolStack> branches = new List<ProtocolStack>();
            branches.Add(textBranch);
            TypeBranchingProtocol branchingProtocol = new TypeBranchingProtocol();
            branchingProtocol.SetBranches(branches);
            state.MiddleProtocols.Add(branchingProtocol);

            // Type tagging
            TypeTagProtocol typeTagProtocol = new TypeTagProtocol();
            state.MiddleProtocols.Add(typeTagProtocol);

            // AES
            AESProtocol aesP = new AESProtocol();
            aesP.SetState((AESProtocolState)_options.SecondLowAESProtocolState.Clone());
            state.MiddleProtocols.Add(aesP);

            BasicSecurityLayer(state, _options.FirstLowAESProtocolState);

            state.Type = DataProtocolType.Text;
            ProtocolStack protocolStack = new ProtocolStack();
            protocolStack.SetState(state);
            return protocolStack;
        }

        private static void BasicSecurityLayer(ProtocolStackState stackState, AESProtocolState aesState)
        {
            // Seq (this protocol will block broadcasted messages)
            stackState.MiddleProtocols.Add(new SequenceProtocol());
            // AES
            AESProtocol aesP = new AESProtocol();
            aesP.SetState((AESProtocolState)aesState.Clone());
            stackState.MiddleProtocols.Add(aesP);
        }

        private ProtocolStack GetBroadcastStack()
        {
            ProtocolStackState state = new ProtocolStackState();

            // broadcast protocol
            BroadcastProtocolState broadcaseState = new BroadcastProtocolState();
            broadcaseState.SockController = _options.SockController;
            broadcaseState.SockMgr = _options.SockMgr;
            state.MiddleProtocols.Add(new BroadcastProtocol(broadcaseState));

            BasicSecurityLayer(state, _options.FirstLowAESProtocolState);

            ProtocolStack protocolStack = new ProtocolStack();
            protocolStack.SetState(state);
            return protocolStack;
        }

        public object Clone()
        {
            DefaultProtocolFactory factory = new DefaultProtocolFactory((DefaultProtocolFactoryOptions)_options.Clone());
            return factory;
        }

        public void SetSockMgr(SockMgr sockMgr)
        {
            _options.SockMgr = sockMgr;
        }
    }
}
