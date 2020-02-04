namespace SocketApp.Protocol
{
    // Block invalid DataContent
    public class BlockProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public void FromHighLayerToHere(DataContent dataContent)
        {
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            if (dataContent.IsAesError || dataContent.IsAckWrong)
                return;
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
