using System;

namespace SocketApp.Protocol
{
    public enum DataProtocolType
    {
        Undefined,
        Text,
        File,
    }
    public class DataContent  // passing through all layers of protocols/middlewares
    {
        public object Data = null;  // the undefined type of data
        public DataProtocolType Type = DataProtocolType.Undefined;
    }

    public delegate void NextLowLayerEventHandler(DataContent dataContent);
    public delegate void NextHighLayerEventHandler(DataContent dataContent);

    public interface IProtocol
    {
        void SetState(object stateObject);
        object GetState();
        // If three hand-shake is needed, then raise `NextLowLayerEvent` rather than `NextHighLayerEvent`
        void FromHighLayerToHere(DataContent dataContent);  // called by the nearby higher layer
        void FromLowLayerToHere(DataContent dataContent);  // called by the nearby lower layer
        event NextLowLayerEventHandler NextLowLayerEvent;  // call the next lower layer
        event NextHighLayerEventHandler NextHighLayerEvent;  // call the next higher layer
    }
}
