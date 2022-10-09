using QIQI.EProjectFile.Encryption;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace QIQI.EProjectFile.Internal
{
    sealed class CryptoECTransform : EStdCryptoTransform
    {
        public CryptoECTransform(EplSecret.EC secret, int lengthOfOvert = 0): base(secret, lengthOfOvert)
        {
        }

        protected override RC4Crypto NextBlockCrypto()
        {
            byte[] blockKey = new byte[40];
            keyTable.Decode(blockKey, 0, 8);
            SecretId.CopyTo(0, blockKey, 8, 32);
            return new RC4Crypto(blockKey, EplSecret.EC.DefaultInitialStatus);
        }
    }
}
