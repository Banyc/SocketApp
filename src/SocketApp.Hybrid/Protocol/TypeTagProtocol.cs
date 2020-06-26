using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketApp.Protocol
{
    // 4-Byte typeHeader to deceide which type the data is
    public class TypeTagProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public void Dispose()
        {
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            byte[] typeHeader;
            typeHeader = BitConverter.GetBytes((int)dataContent.Type);
            List<byte> header_body = new List<byte>();
            header_body.AddRange(typeHeader);
            header_body.AddRange((byte[])dataContent.Data);
            dataContent.Data = header_body.ToArray();
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            byte[] typeHeader;
            byte[] body;
            // TCP segment has not been sufficiently collected
            if ((byte[])dataContent.Data == null)
            {
                dataContent.Type = DataProtocolType.Management;
                NextHighLayerEvent?.Invoke(dataContent);
                return;
            }
            // invalid header
            if (((byte[])dataContent.Data).Length < 4)
            {
                dataContent.IsTypeWrong = true;
                NextHighLayerEvent?.Invoke(dataContent);
                return;
            }
            // first 4 bytes is the type header
            typeHeader = ((byte[])dataContent.Data).Take(4).ToArray();
            body = ((byte[])dataContent.Data).Skip(4).ToArray();
            int typeIndex = BitConverter.ToInt32(typeHeader);
            dataContent.Data = body;
            if (typeIndex >= (int)DataProtocolType.MaxInvalid || typeIndex <= (int)DataProtocolType.Undefined)
            {
                dataContent.IsTypeWrong = true;
                NextHighLayerEvent?.Invoke(dataContent);
                return;  // report if out of range  // it might due to falsely decryption on AES layer
            }
            dataContent.Type = (DataProtocolType)typeIndex;
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
