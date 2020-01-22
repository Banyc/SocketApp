using System.Text;

namespace SocketApp.Protocol
{
    public class UTF8ProtocolState
    {

    }

    public class UTF8Protocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public void FromHighLayerToHere(DataContent arg)
        {
            byte[] data = Encoding.UTF8.GetBytes((string)arg.Data);
            arg.Data = data;
            NextLowLayerEvent?.Invoke(arg);
        }

        public void FromLowLayerToHere(DataContent arg)
        {
            string data = Encoding.UTF8.GetString((byte[])arg.Data);
            arg.Data = data;
            NextHighLayerEvent?.Invoke(arg);
        }

        public object GetState()
        {
            throw new System.NotImplementedException();
        }

        public void SetState(object state)
        {
            throw new System.NotImplementedException();
        }
    }
}
