using System;
namespace SocketApp.Protocol
{
    public class TypeBranchingProtocol : DeliverProtocol
    {
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
