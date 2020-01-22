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

    public delegate void NextLowLayerEventHandler(DataContent data);
    public delegate void NextHighLayerEventHandler(DataContent data);

    public interface IProtocol
    {
        void SetState(object state);
        object GetState();
        // If three hand-shake is needed, then raise `NextLowLayerEvent` rather than `NextHighLayerEvent`
        void FromHighLayerToHere(DataContent data);  // called by the nearby higher layer
        void FromLowLayerToHere(DataContent data);  // called by the nearby lower layer
        event NextLowLayerEventHandler NextLowLayerEvent;  // call the next lower layer
        event NextHighLayerEventHandler NextHighLayerEvent;  // call the next higher layer
    }
}
