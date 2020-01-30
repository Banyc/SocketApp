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
    }
}
