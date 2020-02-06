using System;

namespace SocketApp.Protocol
{
    // Block invalid DataContent
    public class BlockProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public void FromHighLayerToHere(DataContent dataContent)
        {
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            if (!dataContent.IsValid)
            {
                // WORKAROUND - For DEBUG
                Console.WriteLine("[Error]");
                Console.WriteLine($"Ack Wrong {dataContent.IsAckWrong}");
                Console.WriteLine($"AES Error {dataContent.IsAesError}");
                Console.WriteLine($"Heartbeat Timeout {dataContent.IsHeartbeatTimeout}");
                Console.WriteLine($"Timestamp Wrong {dataContent.IsTimestampWrong}");
                
                return;
            }
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
