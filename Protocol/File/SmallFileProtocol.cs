using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SocketApp.Protocol
{
    [Serializable]
    public class SmallFileDataObject : ICloneable
    {
        public string Filename;
        public byte[] BinData;

        public object Clone()
        {
            SmallFileDataObject dataObject = new SmallFileDataObject();
            dataObject.Filename = this.Filename;
            dataObject.BinData = (byte[])this.BinData.Clone();
            return dataObject;
        }
    }

    // __Type of `DataContent.Data`__
    // SmallFileDataObject
    // byte[]

    public class SmallFileProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public void FromHighLayerToHere(DataContent dataContent)
        {
            SmallFileDataObject dataObject = (SmallFileDataObject)dataContent.Data;
            // Convert an object to a byte array
            dataContent.Data = Util.ObjectByteConverter.ObjectToByteArray(dataObject);
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            // Convert a byte array to an Object
            dataContent.Data = (ICloneable)Util.ObjectByteConverter.ByteArrayToObject((byte[])dataContent.Data);
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
