using System.Linq;
using System.Collections.Generic;
namespace SocketApp.Protocol
{
    public class TextProtocolState
    {
        public List<IProtocol<object, object, object>> middleProtocols = new List<IProtocol<object, object, object>>();
    }

    public class TextProtocol : IProtocol<byte[], string, TextProtocolState>
    {
        private TextProtocolState _state = new TextProtocolState();
        public byte[] GetDown(string arg)
        {
            object tmp = arg;
            foreach (var proto in _state.middleProtocols)
            {
                tmp = proto.GetDown(tmp);
            }
            return (byte[])tmp;
        }

        public TextProtocolState GetState()
        {
            return _state;
        }

        public string GoUp(byte[] arg)
        {
            object tmp = arg;
            foreach (var proto in _state.middleProtocols.AsEnumerable().Reverse())
            {
                tmp = proto.GoUp(tmp);
            }
            return (string)tmp;
        }

        public void SetState(TextProtocolState state)
        {
            _state = state;
        }
    }
}
