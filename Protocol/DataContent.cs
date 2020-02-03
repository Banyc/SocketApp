using System;
namespace SocketApp.Protocol
{
    public enum DataProtocolType
    {
        Undefined,
        Text,
        File,
    }
    
    public class DataContent : ICloneable  // passing through all layers of protocols/middlewares
    {
        public ICloneable Data = null;  // the undefined type of data
        public DataProtocolType Type = DataProtocolType.Undefined;  // determine which branch of protocol to go
        public byte[] AesKey = null;  // to update the AesKey through protocol stack
        public bool IsAesError = false;

        public object Clone()
        {
            DataContent dataContent = new DataContent();
            dataContent.Data = (ICloneable)this.Data?.Clone();
            dataContent.Type = this.Type;
            dataContent.AesKey = (byte[])AesKey?.Clone();
            dataContent.IsAesError = this.IsAesError;
            return dataContent;
        }
    }
}
