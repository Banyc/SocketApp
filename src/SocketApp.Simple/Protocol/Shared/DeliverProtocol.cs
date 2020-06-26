using System.Collections.Generic;
namespace SocketApp.Simple.Protocol
{
    // make possible for spliting a protocol stack
    public abstract class DeliverProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        private List<ProtocolStack> _branches = new List<ProtocolStack>();

        public void FromHighLayerToHere(DataContent dataContent)
        {
            this._branches[FromHighLayerToHere_IndexSelection(dataContent)].FromHighLayerToHere(dataContent);
        }
        public void FromLowLayerToHere(DataContent dataContent)
        {
            this._branches[FromLowLayerToHere_IndexSelection(dataContent)].FromLowLayerToHere(dataContent);
        }

        protected abstract int FromHighLayerToHere_IndexSelection(DataContent dataContent);
        protected abstract int FromLowLayerToHere_IndexSelection(DataContent dataContent);

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

        public void Dispose()
        {
            Unlink();
            foreach (ProtocolStack stack in _branches)
            {
                stack.Dispose();
            }
        }
    }
}
