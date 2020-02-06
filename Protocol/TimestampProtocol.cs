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
        public double _extraTolerationInSec;
        public TimestampProtocol(double expireSpeedBytePerSec = 1000, double extraTolerationInSec = 1 * 60)
        {
            _expireSpeedBytePerSec = expireSpeedBytePerSec;
            _extraTolerationInSec = extraTolerationInSec;
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
                double expireInterval = ((byte[])dataContent.Data).Length / _expireSpeedBytePerSec + _extraTolerationInSec;
                if (expireInterval < (DateTime.Now - timestamp).TotalSeconds)
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
