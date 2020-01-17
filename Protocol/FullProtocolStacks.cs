using System.Linq;
using System.Collections.Generic;
namespace SocketApp.Protocol
{
    public class FullProtocolStacksState
    {
        public List<IProtocol> middleProtocols = new List<IProtocol>();
    }

    public class FullProtocolStacks : IProtocol
    {
        private FullProtocolStacksState _state = new FullProtocolStacksState();
        public object GetDown(object arg)
        {
            object tmp = arg;
            foreach (var proto in _state.middleProtocols)
            {
                tmp = proto.GetDown(tmp);
            }
            return (object)tmp;
        }

        public object GetState()
        {
            return _state;
        }

        public object GoUp(object arg)
        {
            object tmp = arg;
            foreach (var proto in _state.middleProtocols.AsEnumerable().Reverse())
            {
                tmp = proto.GoUp(tmp);
            }
            return (string)tmp;
        }

        public void SetState(object state)
        {
            _state = (FullProtocolStacksState)state;
        }
    }
}
