using System.Collections.Generic;
namespace SocketApp.Protocol
{
    // make possible for spliting a protocol stack
    public class DeliverProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        private List<ProtocolStack> _branches = new List<ProtocolStack>();

        public void FromHighLayerToHere(DataContent dataContent)
        {
            this._branches[dataContent.NextLayerIndex].FromHighLayerToHere(dataContent);
        }
        public void FromLowLayerToHere(DataContent dataContent)
        {
            this._branches[dataContent.NextLayerIndex].FromLowLayerToHere(dataContent);
        }

        private void Branch_NextLowLayerEvent(DataContent dataContent)
        {
            NextLowLayerEvent?.Invoke(dataContent);
        }
        private void Branch_NextHighLayerEvent(DataContent dataContent)
        {
            NextHighLayerEvent?.Invoke(dataContent);
        }

        public void SetBranches(List<ProtocolStack> branches)
        {
            Unlink();
            _branches = branches;
            Link();
        }
        public void Unlink()
        {
            foreach (var branch in this._branches)
            {
                branch.NextLowLayerEvent -= Branch_NextLowLayerEvent;
                branch.NextHighLayerEvent -= Branch_NextHighLayerEvent;
            }
        }
        private void Link()
        {
            foreach (var branch in this._branches)
            {
                branch.NextLowLayerEvent += Branch_NextLowLayerEvent;
                branch.NextHighLayerEvent += Branch_NextHighLayerEvent;
            }
        }
    }
}
