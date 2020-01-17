using System.Text;

namespace SocketApp.Protocol
{
    public class UTF8ProtocolState
    {

    }

    public class UTF8Protocol : IProtocol<byte[], string, UTF8ProtocolState>
    {
        public byte[] GetDown(string arg)
        {
            return Encoding.UTF8.GetBytes(arg);
        }

        public UTF8ProtocolState GetState()
        {
            throw new System.NotImplementedException();
        }

        public string GoUp(byte[] arg)
        {
            return Encoding.UTF8.GetString(arg);
        }

        public void SetState(UTF8ProtocolState state)
        {
            throw new System.NotImplementedException();
        }
    }
}
