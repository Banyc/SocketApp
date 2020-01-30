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
        public byte[] AesKey = null;  // to update the AesKey through protocol stack
        public int NextLayerIndex = 0;  // determine which branch of protocol to go
        public bool IsAesError = false;
        public int AesErrorCode;
    }

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
