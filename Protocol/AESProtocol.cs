using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace SocketApp.Protocol
{
    public class AESProtocolState
    {
        public CipherMode Mode = CipherMode.CBC;
        public int KeySize = 128;
        public int BlockSize = 128;
        public int FeedbackSize = 128;
        public PaddingMode Padding = PaddingMode.PKCS7;  // only tested this padding
        public byte[] Key;
        public bool Enabled = false;
    }

    public class AESProtocol : IProtocol
    {
        public event NextLowLayerEventHandler NextLowLayerEvent;
        public event NextHighLayerEventHandler NextHighLayerEvent;

        // two ways to update a key
        // first, by SetState()
        // second, by DataContent (TODO)
        private AESProtocolState _state;

        private Aes _aesAlg = Aes.Create();

        public void FromHighLayerToHere(DataContent dataContent)
        {
            if (_state.Enabled)
            {
                byte[] encrypted = Encrypt((byte[])dataContent.Data);
                dataContent.Data = encrypted;
            }
            NextLowLayerEvent?.Invoke(dataContent);
        }

        public void FromLowLayerToHere(DataContent dataContent)
        {
            if (_state.Enabled)
            {
                try
                {
                    byte[] decrypted;
                    decrypted = Decrypt((byte[])dataContent.Data);
                    dataContent.Data = decrypted;
                }
                catch (CryptographicException)
                {
                    dataContent.IsAesError = true;
                }
            }
            NextHighLayerEvent?.Invoke(dataContent);
        }

        public AESProtocolState GetState()
        {
            return _state;
        }

        public void SetState(AESProtocolState state)
        {
            _state = state;
            if (state.Enabled)
            {
                // the order is important
                // set Key and IV after setting their sizes
                _aesAlg.Mode = _state.Mode;
                _aesAlg.KeySize = _state.KeySize;
                _aesAlg.BlockSize = _state.BlockSize;
                _aesAlg.FeedbackSize = _state.FeedbackSize;
                _aesAlg.Padding = _state.Padding;
                _aesAlg.Key = _state.Key;
            }
        }

        private byte[] Decrypt(byte[] crypto)
        {
            _aesAlg.IV = crypto.Take(_aesAlg.BlockSize / 8).ToArray();
            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = _aesAlg.CreateDecryptor();
            return PerformCryptography(crypto.Skip(_aesAlg.BlockSize / 8).ToArray(), decryptor);
        }

        private byte[] Encrypt(byte[] data)
        {
            _aesAlg.GenerateIV();
            ICryptoTransform encryptor = _aesAlg.CreateEncryptor();
            byte[] encryptedData = PerformCryptography(data, encryptor);

            List<byte> iv_data = new List<byte>();
            iv_data.AddRange(_aesAlg.IV);
            iv_data.AddRange(encryptedData);
            return iv_data.ToArray();
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();  // add padding when encryption
                return ms.ToArray();
            }
        }
    }
}
