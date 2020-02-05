using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketApp.Protocol
{
    // flexible expire time for different lengths
    public class TimestampProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public double _expireSpeedBytePerSec;
        public TimestampProtocol(double expireSpeedBytePerSec = 1)
        {
            _expireSpeedBytePerSec = expireSpeedBytePerSec;
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            List<byte> header_data = new List<byte>();
            byte[] now = BitConverter.GetBytes(DateTime.Now.ToBinary());
            header_data.AddRange(now);
            header_data.AddRange((byte[])dataContent.Data);
            dataContent.Data = header_data.ToArray();
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            if (dataContent.Data == null)
            {
                NextHighLayerEvent?.Invoke(dataContent);
                return;
            }
            try
            {
                byte[] timestampBytes = ((byte[])dataContent.Data).Take(8).ToArray();
                byte[] data = ((byte[])dataContent.Data).Skip(8).ToArray();
                long timestampLong = BitConverter.ToInt64(timestampBytes);
                dataContent.Data = data;
                DateTime timestamp = DateTime.FromBinary(timestampLong);
                // flexible expire time for different lengths
                double averageSpeed = ((byte[])dataContent.Data).Length / (DateTime.Now - timestamp).TotalSeconds;  // B/s
                if (averageSpeed < _expireSpeedBytePerSec)
                {
                    dataContent.IsTimestampWrong = true;
                }
            }
            catch (Exception)
            {
                dataContent.IsTimestampWrong = true;
            }
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
