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
            dataContent.Data = ObjectToByteArray(dataObject);
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            // Convert a byte array to an Object
            dataContent.Data = (ICloneable)ByteArrayToObject((byte[])dataContent.Data);
            NextHighLayerEvent?.Invoke(dataContent);
        }

        // Convert an object to a byte array
        private static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        private static object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
