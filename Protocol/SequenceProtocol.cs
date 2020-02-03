using System.Collections.Generic;
using System;
using System.Linq;

namespace SocketApp.Protocol
{
    // prevent replay attack
    public class SequenceProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;
        // a full message as minimal unit
        private int? _oppAck = null;
        private int? _thisAck = null;

        public SequenceProtocol()
        {
            Random rnd = new Random();
            _thisAck = rnd.Next();
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
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
            seqHeader = ((byte[])dataContent.Data).Take(4).ToArray();
            body = ((byte[])dataContent.Data).Skip(4).ToArray();
            int seq = BitConverter.ToInt32(seqHeader);
            if (_oppAck == null)
                _oppAck = seq;
            if (seq != _oppAck)
                // facing a replay attack
                return;
            ++_oppAck;
            dataContent.Data = body;
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
