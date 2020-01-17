using System.Text;

namespace SocketApp.Protocol
{
    public class UTF8ProtocolState
    {

    }

    public class UTF8Protocol : IProtocol
    {
        public object GetDown(object arg)
        {
            return Encoding.UTF8.GetBytes((string)arg);
        }

        public object GetState()
        {
            throw new System.NotImplementedException();
        }

        public object GoUp(object arg)
        {
            return Encoding.UTF8.GetString((byte[])arg);
        }

        public void SetState(object state)
        {
            throw new System.NotImplementedException();
        }
    }
}
