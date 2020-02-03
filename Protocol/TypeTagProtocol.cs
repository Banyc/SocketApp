using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketApp.Protocol
{
    public class TypeTagProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

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
            typeHeader = ((byte[])dataContent.Data).Take(4).ToArray();
            body = ((byte[])dataContent.Data).Skip(4).ToArray();
            int typeIndex = BitConverter.ToInt32(typeHeader);
            dataContent.Data = body;
            if (typeIndex > (int)DataProtocolType.File || typeIndex < (int)DataProtocolType.Undefined)
                return;  // discard if out of range
            dataContent.Type = (DataProtocolType)typeIndex;
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
