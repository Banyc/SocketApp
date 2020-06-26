using System;
namespace SocketApp.Protocol
{
    // Disconnect when invalid DataContent detected
    public class DisconnectProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        public void Dispose()
        {
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            if (!dataContent.IsValid)
            {
                // WORKAROUND - For DEBUG
                Console.WriteLine("[Error][Disconnect]");
                Console.WriteLine($"Ack Wrong {dataContent.IsAckWrong}");
                Console.WriteLine($"AES Error {dataContent.IsAesError}");
                Console.WriteLine($"Heartbeat Timeout {dataContent.IsHeartbeatTimeout}");
                Console.WriteLine($"Timestamp Wrong {dataContent.IsTimestampWrong}");
                Console.WriteLine($"IsTypeWrong {dataContent.IsTypeWrong}");
                Console.Write("> ");

                dataContent.SockMgr?.Shutdown();
                return;
            }
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
