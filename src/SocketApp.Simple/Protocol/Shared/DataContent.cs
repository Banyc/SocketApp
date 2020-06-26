using System;
namespace SocketApp.Simple.Protocol
{
    public enum DataProtocolType
    {
        Undefined,
        Text,
        SmallFile,
        Management,
        MaxInvalid
    }

    [Serializable]
    public class TransportState : ICloneable
    {
        public int PendingLength = 0;  // in bufferMgr
        public int ReceivedLength = 0;  // in bufferMgr
        public double Speed = 0;  // KB/s

        public object Clone()
        {
            TransportState state = new TransportState();
            state.PendingLength = this.PendingLength;
            state.ReceivedLength = this.ReceivedLength;
            state.Speed = this.Speed;
            return state;
        }
    }

    public class DataContent : ICloneable  // passing through all layers of protocols/middlewares
    {
        public SockController SockController = null;
        public SockMgr SockMgr = null;
        public ICloneable Data = null;  // the undefined type of data
        public bool IsShutdown = false;
        public DataProtocolType Type = DataProtocolType.Undefined;  // determine which branch of protocol to go
        public byte[] AesKey = null;  // to update the AesKey through protocol stack
        public bool IsAesError = false;
        public bool IsAckWrong = false;
        public TransportState TransportState = new TransportState();
        public bool IsHeartbeatTimeout = false;
        public bool IsTimestampWrong = false;
        public bool IsTypeWrong = false;
        public bool IsValid
        {
            get
            {
                return !IsAesError && !IsAckWrong && !IsHeartbeatTimeout && !IsTimestampWrong && !IsTypeWrong;
            }
        }
        // hint: add necessary field here

        public object Clone()
        {
            DataContent dataContent = new DataContent();
            dataContent.SockController = this.SockController;
            dataContent.SockMgr = this.SockMgr;
            dataContent.Data = (ICloneable)this.Data?.Clone();
            dataContent.IsShutdown = this.IsShutdown;
            dataContent.Type = this.Type;
            dataContent.AesKey = (byte[])AesKey?.Clone();
            dataContent.IsAesError = this.IsAesError;
            dataContent.IsAckWrong = this.IsAckWrong;
            dataContent.TransportState = (TransportState)this.TransportState.Clone();
            dataContent.IsHeartbeatTimeout = this.IsHeartbeatTimeout;
            dataContent.IsTimestampWrong = this.IsTimestampWrong;
            dataContent.IsTypeWrong = this.IsTypeWrong;
            return dataContent;
        }
    }
}
