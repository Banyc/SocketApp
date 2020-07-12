using System.Text;

namespace SocketApp.ProtocolStack.Protocol
{
    public class UTF8ProtocolState
    {

    }

    public class UTF8Protocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public void Dispose()
        {
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            byte[] data = Encoding.UTF8.GetBytes((string)dataContent.Data);
            dataContent.Data = data;
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            if (dataContent.Data == null)
            {
                dataContent.Data = "<Unintelligible>";
                NextHighLayerEvent?.Invoke(dataContent);
                return;
            }
            string data = Encoding.UTF8.GetString((byte[])dataContent.Data);
            dataContent.Data = data;
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
