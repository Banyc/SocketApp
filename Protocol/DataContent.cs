using System;
namespace SocketApp.Protocol
{
    public enum DataProtocolType
    {
        Undefined,
        Text,
        SmallFile,
    }
    
    public class DataContent : ICloneable  // passing through all layers of protocols/middlewares
    {
        public ICloneable Data = null;  // the undefined type of data
        public DataProtocolType Type = DataProtocolType.Undefined;  // determine which branch of protocol to go
        public byte[] AesKey = null;  // to update the AesKey through protocol stack
        public bool IsAesError = false;
        public SockBase.SocketSendEventHandler ExternalCallback = null;
        public object ExternalCallbackState = null;

        public object Clone()
        {
            DataContent dataContent = new DataContent();
            dataContent.Data = (ICloneable)this.Data?.Clone();
            dataContent.Type = this.Type;
            dataContent.AesKey = (byte[])AesKey?.Clone();
            dataContent.IsAesError = this.IsAesError;
            dataContent.ExternalCallback = this.ExternalCallback;
            dataContent.ExternalCallbackState = this.ExternalCallbackState;
            return dataContent;
        }
    }
}
