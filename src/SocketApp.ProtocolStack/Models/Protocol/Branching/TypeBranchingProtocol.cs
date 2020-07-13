using System;
using System.Collections.Generic;

namespace SocketApp.ProtocolStack.Protocol
{
    public class TypeBranchingProtocol : DeliverProtocol
    {
        public TypeBranchingProtocol(List<ProtocolStack> branches) : base(branches)
        {
            
        }

        protected override int FromHighLayerToHere_IndexSelection(DataContent dataContent)
        {
            return GetTypeIndex(dataContent);
        }

        protected override int FromLowLayerToHere_IndexSelection(DataContent dataContent)
        {
            return GetTypeIndex(dataContent);
        }

        private static int GetTypeIndex(DataContent dataContent)
        {
            return (int)dataContent.Type - 1;
        }
    }
}
