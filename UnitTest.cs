namespace SocketApp
{
    public class UnitTest
    {
        Protocol.AESProtocol _aesProtocol;
        public void Test()
        {
            // AES
            _aesProtocol = new Protocol.AESProtocol();
            Protocol.AESProtocolState aesState = new Protocol.AESProtocolState();
            aesState.Key = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _aesProtocol.SetState(aesState);
            _aesProtocol.NextLowLayerEvent += OnNextLowLayerEvent;
            _aesProtocol.NextHighLayerEvent += OnNextHighLayerEvent;

            // byte[] data = new byte[16] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
            byte[] data = new byte[4] { 0x01, 0x02, 0x03, 0x04 };
            // byte[] data = new byte[17] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02 };
            Protocol.DataContent dataContent = new Protocol.DataContent();
            dataContent.Data = data;
            _aesProtocol.FromHighLayerToHere(dataContent);

        }

        private void OnNextLowLayerEvent(Protocol.DataContent dataContent)
        {
            _aesProtocol.FromLowLayerToHere(dataContent);
        }

        private void OnNextHighLayerEvent(Protocol.DataContent dataContent)
        {

        }
    }
}
