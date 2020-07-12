using System;
using System.Linq;
using System.Collections.Generic;
namespace SocketApp.ProtocolStack.Protocol
{
    public class ProtocolStackOptions : IDisposable
    {
        public List<IProtocol> MiddleProtocols = new List<IProtocol>();  // from high to low layer

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
        private ProtocolStackOptions _options = new ProtocolStackOptions();

        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public ProtocolStack(ProtocolStackOptions options)
        {
            _options = options;
        }

        private void LinkMiddleProtocols()
        {
            _options.LinkMiddleProtocols();
            int i;
            for (i = 0; i < _options.MiddleProtocols.Count; ++i)
            {
                if (i + 1 < _options.MiddleProtocols.Count)
                {
                }
                else  // already the last element
                {
                    _options.MiddleProtocols[i].NextLowLayerEvent += OnNextLowLayerEvent;
                }
                if (i > 0)
                {
                }
                else  // the first element
                {
                    _options.MiddleProtocols[i].NextHighLayerEvent += OnNextHighLayerEvent;
                }
            }
        }

        private void UnlinkMiddleProtocols()
        {
            _options.UnlinkMiddleProtocols();
            int i;
            for (i = 0; i < _options.MiddleProtocols.Count; ++i)
            {
                if (i + 1 < _options.MiddleProtocols.Count)
                {
                }
                else  // already the last element
                {
                    _options.MiddleProtocols[i].NextLowLayerEvent -= OnNextLowLayerEvent;
                }
                if (i > 0)
                {
                }
                else  // the first element
                {
                    _options.MiddleProtocols[i].NextHighLayerEvent -= OnNextHighLayerEvent;
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
            _options.MiddleProtocols.First().FromHighLayerToHere(data);
        }

        public void FromLowLayerToHere(DataContent data)
        {
            _options.MiddleProtocols.Last().FromLowLayerToHere(data);
        }

        public void Dispose()
        {
            UnlinkMiddleProtocols();
            _options.Dispose();
        }
    }
}
