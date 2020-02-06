using System.Threading;
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

        private ProtocolStack GetFileBranch()
        {
            // File Branch
            ProtocolStackState fileBranchState = new ProtocolStackState();
            fileBranchState.MiddleProtocols.Add(new SmallFileProtocol());  // SmallFileProtocol
            ProtocolStack fileBranch = new ProtocolStack();
            fileBranch.SetState(fileBranchState);
            fileBranchState.Type = DataProtocolType.SmallFile;
            return fileBranch;
        }
        private ProtocolStack GetTextBranch()
        {
            // Text Branch
            ProtocolStackState textBranchState = new ProtocolStackState();
            textBranchState.MiddleProtocols.Add(new UTF8Protocol());  // UTF8
            ProtocolStack textBranch = new ProtocolStack();
            textBranch.SetState(textBranchState);
            textBranchState.Type = DataProtocolType.Text;
            return textBranch;
        }
        private ProtocolStack GetManagementBranch()
        {
            ProtocolStackState managementBranchState = new ProtocolStackState();
            managementBranchState.MiddleProtocols.Add(new TransportInfoProtocol());  // Transport Info
            ProtocolStack managementBranch = new ProtocolStack();
            managementBranch.SetState(managementBranchState);
            managementBranchState.Type = DataProtocolType.Management;
            return managementBranch;
        }
        private ProtocolStack GetDefaultStack()
        {
            ProtocolStackState state = new ProtocolStackState();

            // branching
            List<ProtocolStack> branches = new List<ProtocolStack>();
            branches.Add(GetTextBranch());  // index 0
            branches.Add(GetFileBranch());  // index 1
            branches.Add(GetManagementBranch());  // index 2
            TypeBranchingProtocol branchingProtocol = new TypeBranchingProtocol();
            branchingProtocol.SetBranches(branches);
            state.MiddleProtocols.Add(branchingProtocol);

            // Block invalid DataContent
            state.MiddleProtocols.Add(new BlockProtocol());
            // Type tagging  // blocking false decryption
            TypeTagProtocol typeTagProtocol = new TypeTagProtocol();
            state.MiddleProtocols.Add(typeTagProtocol);

            // Block invalid DataContent
            state.MiddleProtocols.Add(new BlockProtocol());
            // AES
            AESProtocol aesP = new AESProtocol();
            aesP.SetState((AESProtocolState)_options.SecondLowAESProtocolState.Clone());
            state.MiddleProtocols.Add(aesP);

            // Basic Security Layers
            AddBasicSecurityLayer(state, _options.FirstLowAESProtocolState);

            state.Type = DataProtocolType.Text;
            ProtocolStack protocolStack = new ProtocolStack();
            protocolStack.SetState(state);
            return protocolStack;
        }

        private static void AddBasicSecurityLayer(ProtocolStackState stackState, AESProtocolState aesState)
        {
            // ordering
            ManualResetEvent topDownOrdering = new ManualResetEvent(true);
            ManualResetEvent buttomUpOrdering = new ManualResetEvent(true);

            // Block invalid data and report
            stackState.MiddleProtocols.Add(new BlockProtocol());
            // Heartbeat
            stackState.MiddleProtocols.Add(new HeartbeatProtocol());
            // Timestamp
            stackState.MiddleProtocols.Add(new TimestampProtocol());
            // Seq (this protocol will block broadcasted messages)  // identify false decryption  // TODO: replace it with challenge
            stackState.MiddleProtocols.Add(new SequenceProtocol(topDownOrdering, buttomUpOrdering));
            // AES
            AESProtocol aesP = new AESProtocol();
            aesP.SetState((AESProtocolState)aesState.Clone());
            stackState.MiddleProtocols.Add(aesP);
            // Framing
            stackState.MiddleProtocols.Add(new FramingProtocol(topDownOrdering, buttomUpOrdering));
        }

        private ProtocolStack GetBroadcastStack()
        {
            ProtocolStackState state = new ProtocolStackState();

            // broadcast protocol
            BroadcastProtocolState broadcaseState = new BroadcastProtocolState();
            broadcaseState.SockController = _options.SockController;
            broadcaseState.SockMgr = _options.SockMgr;
            state.MiddleProtocols.Add(new BroadcastProtocol(broadcaseState));

            // Disconnect when dataContent invalid and report
            state.MiddleProtocols.Add(new DisconnectProtocol());
            AddBasicSecurityLayer(state, _options.FirstLowAESProtocolState);

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
