namespace SocketApp.Protocol
{
    public delegate void NextLowLayerEventHandler(DataContent dataContent);
    public delegate void NextHighLayerEventHandler(DataContent dataContent);

    public interface IProtocol
    {
        // If three hand-shake is needed, then raise `NextLowLayerEvent` rather than `NextHighLayerEvent`
        void FromHighLayerToHere(DataContent dataContent);  // called by the nearby higher layer
        void FromLowLayerToHere(DataContent dataContent);  // called by the nearby lower layer
        event NextLowLayerEventHandler NextLowLayerEvent;  // call the next lower layer
        event NextHighLayerEventHandler NextHighLayerEvent;  // call the next higher layer
    }
}
