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
        public DataProtocolType Type = DataProtocolType.Undefined;  // determine which branch of protocol to go
        public byte[] AesKey = null;  // to update the AesKey through protocol stack
        public bool IsAesError = false;
    }
}
