using System;
using System.Linq;
using System.Collections.Generic;
namespace SocketApp.Protocol
{
    public class ProtocolStackState : IDisposable
    {
        public List<IProtocol> MiddleProtocols = new List<IProtocol>();  // from high to low layer
        public DataProtocolType Type = DataProtocolType.Undefined;  // deprecated

        public void Dispose()
        {
            foreach (IProtocol protocol in MiddleProtocols)
            {
                protocol.Dispose();
            }
        }

        public void LinkMiddleProtocols()
        {
            int i;
            for (i = 0; i < MiddleProtocols.Count; ++i)
            {
                if (i + 1 < MiddleProtocols.Count)
                {
                    MiddleProtocols[i].NextLowLayerEvent += MiddleProtocols[i + 1].FromHighLayerToHere;
                }
                if (i > 0)
                {
                    MiddleProtocols[i].NextHighLayerEvent += MiddleProtocols[i - 1].FromLowLayerToHere;
                }
            }
        }

        public void UnlinkMiddleProtocols()
        {
            int i;
            for (i = 0; i < MiddleProtocols.Count; ++i)
            {
                if (i + 1 < MiddleProtocols.Count)
                {
                    MiddleProtocols[i].NextLowLayerEvent -= MiddleProtocols[i + 1].FromHighLayerToHere;
                }
                if (i > 0)
                {
                    MiddleProtocols[i].NextHighLayerEvent -= MiddleProtocols[i - 1].FromLowLayerToHere;
                }
            }
        }
    }

    // a stack of protocols
    // a full protocol stack that contains protocols/middlewares
    public class ProtocolStack : IProtocol
    {
        private ProtocolStackState _state = new ProtocolStackState();

        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;
        private void LinkMiddleProtocols()
        {
            _state.LinkMiddleProtocols();
            int i;
            for (i = 0; i < _state.MiddleProtocols.Count; ++i)
            {
                if (i + 1 < _state.MiddleProtocols.Count)
                {
                }
                else  // already the last element
                {
                    _state.MiddleProtocols[i].NextLowLayerEvent += OnNextLowLayerEvent;
                }
                if (i > 0)
                {
                }
                else  // the first element
                {
                    _state.MiddleProtocols[i].NextHighLayerEvent += OnNextHighLayerEvent;
                }
            }
        }

        private void UnlinkMiddleProtocols()
        {
            _state.UnlinkMiddleProtocols();
            int i;
            for (i = 0; i < _state.MiddleProtocols.Count; ++i)
            {
                if (i + 1 < _state.MiddleProtocols.Count)
                {
                }
                else  // already the last element
                {
                    _state.MiddleProtocols[i].NextLowLayerEvent -= OnNextLowLayerEvent;
                }
                if (i > 0)
                {
                }
                else  // the first element
                {
                    _state.MiddleProtocols[i].NextHighLayerEvent -= OnNextHighLayerEvent;
                }
            }
        }

        private void OnNextLowLayerEvent(DataContent data)
        {
            NextLowLayerEvent?.Invoke(data);
        }
        private void OnNextHighLayerEvent(DataContent data)
        {
            NextHighLayerEvent?.Invoke(data);
        }

        public void FromHighLayerToHere(DataContent data)
        {
            _state.MiddleProtocols.First().FromHighLayerToHere(data);
        }

        public void FromLowLayerToHere(DataContent data)
        {
            _state.MiddleProtocols.Last().FromLowLayerToHere(data);
        }

        // remove all Event chains
        public void RemoveEventChains()
        {
            UnlinkMiddleProtocols();
        }

        public ProtocolStackState GetState()
        {
            return _state;
        }

        // setup Event chains
        public void SetState(ProtocolStackState state)
        {
            UnlinkMiddleProtocols();
            _state = state;
            LinkMiddleProtocols();
        }

        public void Dispose()
        {
            UnlinkMiddleProtocols();
            _state.Dispose();
        }
    }
}
