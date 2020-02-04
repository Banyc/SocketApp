using System;
using System.Collections.Generic;

namespace SocketApp.Protocol
{
    public class FramingProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;
        private BufferMgr _bufferMgr = new BufferMgr();
        public FramingProtocol() { }

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
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
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
        }
    }
}
