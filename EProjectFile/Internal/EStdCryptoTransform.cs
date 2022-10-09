using QIQI.EProjectFile.Encryption;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace QIQI.EProjectFile.Internal
{
    internal class EStdCryptoTransform : ICryptoTransform
    {
        private const int BlockLength = 4096;
        protected readonly RC4Crypto keyTable;

        public ImmutableArray<byte> SecretId { get; }
        private int lengthOfRemainedOvert = 0;

        public EStdCryptoTransform(EplSecret.EStd secret, int lengthOfOvert = 0): this((EplSecret)secret, lengthOfOvert)
        {
        }

        protected EStdCryptoTransform(EplSecret secret, int lengthOfOvert = 0)
        {
            if (secret is null)
            {
                throw new ArgumentNullException(nameof(secret));
            }
            if (lengthOfOvert < 0)
            {
                throw new ArgumentException(nameof(lengthOfOvert));
            }
            keyTable = new RC4Crypto(default, secret.IV);
            SecretId = secret.SecretId;
            lengthOfRemainedOvert = lengthOfOvert;
        }

        public bool CanReuseTransform => false;

        public bool CanTransformMultipleBlocks => false;

        public int InputBlockSize => BlockLength;

        public int OutputBlockSize => BlockLength;

        private int blockIndex = 0;

        public void Dispose()
        {
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            var crpyto = NextBlockCrypto();
            if (inputCount > lengthOfRemainedOvert)
            {
                crpyto.Decode(outputBuffer, outputOffset + lengthOfRemainedOvert, inputCount - lengthOfRemainedOvert);
                lengthOfRemainedOvert = 0;
            }
            else
            {
                lengthOfRemainedOvert -= inputCount;
            }
            return inputCount;
        }

        protected virtual RC4Crypto NextBlockCrypto()
        {
            byte[] blockKey = new byte[40];
            keyTable.Decode(blockKey, 0, 4);
            var nPrefix = blockKey[0] + (blockKey[1] << 8) + (blockKey[2] << 16) + (blockKey[3] << 24);
            var nSuffix = blockIndex ^ nPrefix;
            blockIndex++;
            SecretId.CopyTo(0, blockKey, 4, 32);
            blockKey[36] = unchecked((byte)(nSuffix & 0xFF));
            blockKey[37] = unchecked((byte)((nSuffix >> 8) & 0xFF));
            blockKey[38] = unchecked((byte)((nSuffix >> 16) & 0xFF));
            blockKey[39] = unchecked((byte)((nSuffix >> 24) & 0xFF));
            var crypto = new RC4Crypto(blockKey, 256);
            crypto.Skip(36); // Magic
            return crypto;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var buf = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, buf, 0);
            return buf;
        }
    }
}
