namespace SocketApp.Protocol
{
    // Disconnect when invalid DataContent detected
    public class DisconnectProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public void FromHighLayerToHere(DataContent dataContent)
        {
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            if (dataContent.IsAesError || dataContent.IsAckWrong || dataContent.IsHeartbeatTimeout)
            {
                dataContent.SockMgr?.Shutdown();
                return;
            }
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
