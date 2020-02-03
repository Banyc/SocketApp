namespace SocketApp.Protocol
{
    public class TypeBranchingProtocol : DeliverProtocol
    {
        protected override int FromHighLayerToHere_IndexSelection(DataContent dataContent)
        {
            return (int)dataContent.Type - 1;
        }

        protected override int FromLowLayerToHere_IndexSelection(DataContent dataContent)
        {
            return (int)dataContent.Type - 1;
        }
    }
}
