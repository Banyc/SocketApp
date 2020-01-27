using Microsoft.VisualBasic.CompilerServices;
namespace SocketApp.Protocol
{
    public class BroadcastProtocolState
    {
        public SockController SockController;
        public SockMgr SockMgr;
        public AESProtocolState AesState;
    }
    
    // __data unit for client
    // 
    // string - for higher layer
    // byte[] - for lower layer

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
        // this protocol stack includes UTF8 and AES
        ProtocolStack _stackForClient = null;

        public BroadcastProtocol(BroadcastProtocolState state)
        {
            _state = state;

            if (_state.AesState.Key == null)
            {
                // it is a server to broadcast

            }
            else  // it is a client
            {
                // make UTF8 layer
                UTF8Protocol utf8 = new UTF8Protocol();
                // make AES layer
                AESProtocol aesP = new AESProtocol();
                aesP.SetState(_state.AesState);
                // combine
                ProtocolStackState stackState = new ProtocolStackState();
                stackState.Type = DataProtocolType.Text;
                stackState.MiddleProtocols.Add(utf8);
                stackState.MiddleProtocols.Add(aesP);
                _stackForClient = new ProtocolStack();
                _stackForClient.SetState(stackState);
                _stackForClient.NextHighLayerEvent += OnNextHighLayerEvent_stackForClient;
                _stackForClient.NextLowLayerEvent += OnNextLowLayerEvent_stackForClient;
            }
        }

        private void OnNextLowLayerEvent_stackForClient(DataContent dataContent)
        {
            NextLowLayerEvent?.Invoke(dataContent);
        }
        private void OnNextHighLayerEvent_stackForClient(DataContent dataContent)
        {
            NextHighLayerEvent?.Invoke(dataContent);
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            if (_stackForClient == null)  // server relay the message
            {
                if (dataContent.Data.GetType() == typeof(byte[]))
                    NextLowLayerEvent?.Invoke(dataContent);
                // discard string message
            }
            else  // clients send message
            {
                _stackForClient.FromHighLayerToHere(dataContent);
            }
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            if (_stackForClient == null)
            {
                // server broadcast
                foreach (SockMgr client in _state.SockController.GetSockList().Clients)
                {
                    // don't re-sent to source
                    if (client == _state.SockMgr)
                        continue;
                    // find the same class from other peer sockets and activate them
                    ProtocolStackState peerStackState = (ProtocolStackState) client.GetProtocolList().Text.GetState();
                    if (typeof(BroadcastProtocol) == peerStackState.MiddleProtocols[0]?.GetType())
                    {
                        peerStackState.MiddleProtocols[0].FromHighLayerToHere(dataContent);
                    }
                }
            }
            else
            {
                // client try to decrypt
                _stackForClient.FromLowLayerToHere(dataContent);
            }
        }

        public object GetState()
        {
            throw new System.NotImplementedException();
        }

        public void SetState(object stateObject)
        {
            throw new System.NotImplementedException();
        }
    }
}
