using Microsoft.VisualBasic.CompilerServices;
namespace SocketApp.Protocol
{
    public class BroadcastProtocolState
    {
        public SockController SockController;
        public SockMgr SockMgr;
    }

    // __data unit for server
    // 
    // <not sending up> - for higher layer
    // byte[] - for lower layer

    // this protocol allows you to set the server as a message relay to all connected clients
    public class BroadcastProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;
        BroadcastProtocolState _state;

        public BroadcastProtocol(BroadcastProtocolState state)
        {
            _state = state;
        }

        private void OnNextLowLayerEvent_stackForClient(DataContent dataContent)
        {
            NextLowLayerEvent?.Invoke(dataContent);
        }
        private void OnNextHighLayerEvent_stackForClient(DataContent dataContent)
        {
            NextHighLayerEvent?.Invoke(dataContent);
        }

        // forwarding
        public void FromHighLayerToHere(DataContent dataContent)
        {
            // server relay the message
            if (dataContent.Data.GetType() == typeof(byte[]))
                NextLowLayerEvent?.Invoke(dataContent);
            // discard string message
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            // server start broadcasting
            foreach (SockMgr client in _state.SockController.GetSockList().Clients)
            {
                // don't re-sent to source
                if (client == _state.SockMgr)
                    continue;
                // find the same class from other peer sockets and activate them
                ProtocolStackState peerStackState = client.GetProtocolStack().GetState();
                if (typeof(BroadcastProtocol) == peerStackState.MiddleProtocols[0]?.GetType())
                {
                    peerStackState.MiddleProtocols[0].FromHighLayerToHere((SocketApp.Protocol.DataContent)dataContent.Clone());
                }
            }
        }
    }
}
