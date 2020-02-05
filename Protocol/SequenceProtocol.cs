using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SocketApp.Protocol
{
    // prevent replay attack
    // Challenge–response authentication
    public class SequenceProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;
        // a full message as minimal unit
        private int? _oppAck = null;
        private int? _thisAck = null;
        ManualResetEvent _topDownOrdering;
        ManualResetEvent _buttomUpOrdering;

        public SequenceProtocol(ManualResetEvent topDownOrdering, ManualResetEvent buttomUpOrdering)
        {
            Random rnd = new Random();
            _thisAck = rnd.Next();
            _topDownOrdering = topDownOrdering;
            _buttomUpOrdering = buttomUpOrdering;
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            try
            {
                _topDownOrdering.WaitOne();
            }
            catch (AbandonedMutexException)
            {
                // The wait completed due to an abandoned mutex.
                // specific reason unknown
                // Workaround
            }
            byte[] seqHeader;
            seqHeader = BitConverter.GetBytes(_thisAck.Value);
            List<byte> header_body = new List<byte>();
            header_body.AddRange(seqHeader);
            header_body.AddRange((byte[])dataContent.Data);
            dataContent.Data = header_body.ToArray();
            ++_thisAck;
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
                _buttomUpOrdering.Set();
                return;
            }
            try
            {
                seqHeader = ((byte[])dataContent.Data).Take(4).ToArray();
                body = ((byte[])dataContent.Data).Skip(4).ToArray();
                seq = BitConverter.ToInt32(seqHeader);
                if (_oppAck == null)
                    _oppAck = seq;
                if (seq != _oppAck)
                {
                    dataContent.IsAckWrong = true;
                }
                if (seq == _oppAck)
                {
                    ++_oppAck;
                    dataContent.Data = body;
                }
            }
            catch (Exception)
            {
                dataContent.IsAckWrong = true;
            }
            _buttomUpOrdering.Set();
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
