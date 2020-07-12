using System;
namespace SocketApp.ProtocolStack.Protocol
{
    public class TransportInfoProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        private int _prevReceivedLength = 0;
        private DateTime _prevTime;
        private double _prevSpeed = 0;  // KB/s
        private const double alpha = 0.1;

        public TransportInfoProtocol()
        {
            _prevTime = DateTime.Now;
        }

        public void FromHighLayerToHere(DataContent dataContent)
        {
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            TransportState state = dataContent.TransportState;
            // not receiving enough (this is receiver)
            if (dataContent.Data == null)
            {
                //  reset
                if (state.PendingLength == 0)
                {
                    _prevReceivedLength = state.ReceivedLength;
                    _prevTime = DateTime.Now;
                    _prevSpeed = 0;
                    return;
                }
                //  count speed
                if (DateTime.Now == _prevTime)
                {
                    state.Speed = double.MaxValue;
                }
                else
                {
                    state.Speed = (1 - alpha) * _prevSpeed + alpha * (state.ReceivedLength - _prevReceivedLength) / 1024 / (DateTime.Now - _prevTime).TotalSeconds;
                }
                //  update
                _prevSpeed = state.Speed;
                _prevReceivedLength = state.ReceivedLength;
                _prevTime = DateTime.Now;
                // write state in dataContent
                Byte[] data = Util.ObjectByteConverter.ObjectToByteArray(state);
                dataContent.Data = data;
                // tell peer about the transport process
                NextLowLayerEvent?.Invoke((DataContent)dataContent.Clone());
            }
            // still sending (this is sender)
            else
            {
                // get tranport process info from receiver
                dataContent.TransportState = (TransportState)Util.ObjectByteConverter.ByteArrayToObject((byte[])dataContent.Data);
            }
            // tell this app
            NextHighLayerEvent?.Invoke(dataContent);
        }

        public void Dispose()
        {
        }
    }
}
