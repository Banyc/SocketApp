using System;
using System.Collections.Generic;
using System.Threading;

namespace SocketApp.Protocol
{
    public class FramingProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;
        private Util.BufferMgr _bufferMgr = new Util.BufferMgr();
        ManualResetEvent _topDownOrdering;
        ManualResetEvent _buttomUpOrdering;
        public FramingProtocol(ManualResetEvent topDownOrdering, ManualResetEvent buttomUpOrdering)
        {
            _topDownOrdering = topDownOrdering;
            _buttomUpOrdering = buttomUpOrdering;
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            byte[] data = (byte[])dataContent.Data;
            int length = data.Length;
            byte[] lengthByte = BitConverter.GetBytes(length);  // 4 Bytes
            List<byte> prefix_data = new List<byte>();
            prefix_data.AddRange(lengthByte);
            prefix_data.AddRange(data);
            dataContent.Data = prefix_data.ToArray();
            NextLowLayerEvent?.Invoke(dataContent);
            _topDownOrdering.Set();
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            try
            {
                _buttomUpOrdering.WaitOne();
            }
            catch (AbandonedMutexException)
            {
                // Workaround; in case socket shutdown
            }
            _bufferMgr.AddBytes((byte[])dataContent.Data, ((byte[])dataContent.Data).Length);
            dataContent.Data = null;

            byte[] data = _bufferMgr.GetAdequateBytes();
            while (data.Length > 0)
            {
                DataContent newDataContent = (DataContent)dataContent.Clone();
                newDataContent.Data = data;
                NextHighLayerEvent?.Invoke(newDataContent);

                data = _bufferMgr.GetAdequateBytes();
            }

            dataContent.TransportState.PendingLength = _bufferMgr.GetPendingLength();
            dataContent.TransportState.ReceivedLength = _bufferMgr.GetReceivedLength();
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
