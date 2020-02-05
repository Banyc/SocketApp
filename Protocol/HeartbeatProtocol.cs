using System;
using System.Timers;

namespace SocketApp.Protocol
{
    public class HeartbeatProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        private Timer _heartbeatTimer = new Timer();
        private Timer _timeoutTimer = new Timer();

        public HeartbeatProtocol(double heartbeatInterval = 60 * 1000, double timeoutInterval = 200 * 1000)
        {
            _heartbeatTimer.Interval = heartbeatInterval;
            _timeoutTimer.Interval = timeoutInterval;
            _heartbeatTimer.AutoReset = true;
            _timeoutTimer.AutoReset = true;
            _heartbeatTimer.Elapsed += _heartbeatTimer_Elapsed;
            _timeoutTimer.Elapsed += _timeoutTimer_Elapsed;
            _heartbeatTimer.Start();
        }

        private void _timeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DataContent dataContent = new DataContent();
            dataContent.IsHeartbeatTimeout = true;
            NextHighLayerEvent?.Invoke(dataContent);
        }

        private void _heartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DataContent dataContent = new DataContent();
            dataContent.Data = new byte[0];  // send empty message
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            _heartbeatTimer.Stop();
            NextLowLayerEvent?.Invoke(dataContent);
            _heartbeatTimer.Start();
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            _timeoutTimer.Stop();
            _timeoutTimer.Start();
            if (((byte[])dataContent.Data).Length == 0)  // discard empty message
                return;
            NextHighLayerEvent?.Invoke(dataContent);
        }
    }
}
