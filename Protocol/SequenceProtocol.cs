using System.Collections.Generic;
using System;
using System.Linq;
using SocketApp.Util;

namespace SocketApp.Protocol
{
    // prevent replay attack
    // Challenge–response authentication
    // size of sending window: 1; size of receiving window: `windowSize`
    public class SequenceProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;
        // a full message as minimal unit
        private int? _thisSeq = null;
        // ~~Notice: the window is NOT necessarily an internally contiguous window~~
        private SlidingWindow _receiveWindow = null;
        private int _windowSize;

        public SequenceProtocol(int windowSize = 16)
        {
            Random rnd = new Random();
            _thisSeq = rnd.Next();
            _windowSize = windowSize;
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            byte[] seqHeader;
            seqHeader = BitConverter.GetBytes(_thisSeq.Value);
            List<byte> header_body = new List<byte>();
            header_body.AddRange(seqHeader);
            header_body.AddRange((byte[])dataContent.Data);
            dataContent.Data = header_body.ToArray();
            ++_thisSeq;
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            byte[] seqHeader;
            byte[] body;
            int seq;
            // it is a state only packet
            if (dataContent.Data == null)
            {
                NextHighLayerEvent?.Invoke(dataContent);
                return;
            }
            try
            {
                // parse header
                seqHeader = ((byte[])dataContent.Data).Take(4).ToArray();
                body = ((byte[])dataContent.Data).Skip(4).ToArray();
                seq = BitConverter.ToInt32(seqHeader);
                // remove header from dataContent
                dataContent.Data = body;
                // init
                if (_receiveWindow == null)
                    _receiveWindow = new SlidingWindow(seq, _windowSize);
                // check validation
                if (!_receiveWindow.IsValid(seq))
                {
                    dataContent.IsAckWrong = true;
                }
                // update
                _receiveWindow.Update(seq);
            }
            catch (Exception)
            {
                dataContent.IsAckWrong = true;
            }
            NextHighLayerEvent?.Invoke(dataContent);
        }

    }
}
