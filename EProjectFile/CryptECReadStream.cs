using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QIQI.EProjectFile
{
    internal class CryptECReadStream : Stream
    {
        private static readonly byte[] initialStatus = new byte[] {
            0xF0, 0x5E, 0x99, 0xA1, 0x88, 0xE3, 0x1E, 0xEE, 0x11, 0x9E, 0xC9, 0x97, 0x1B, 0x90, 0x4F, 0x7C,
            0x52, 0xCB, 0x82, 0xFA, 0x27, 0xDE, 0xF6, 0xA8, 0xDA, 0xD3, 0xB0, 0xCF, 0x56, 0xD6, 0x85, 0x42,
            0x1A, 0x9C, 0xB5, 0x0E, 0xB8, 0xED, 0x10, 0x1C, 0x24, 0x6A, 0x69, 0xCE, 0x87, 0x55, 0x1F, 0x96,
            0x6C, 0x7B, 0xBA, 0x65, 0x14, 0xAA, 0x2C, 0xDD, 0xA3, 0xB6, 0x7D, 0x63, 0xF5, 0xE9, 0x8E, 0x20,
            0x41, 0x23, 0x78, 0x8C, 0xFC, 0x22, 0x9F, 0xA6, 0xB4, 0x6F, 0xA7, 0x77, 0x59, 0xC0, 0xBF, 0x3A,
            0x30, 0xA2, 0x15, 0x2A, 0x53, 0x5D, 0x74, 0x4D, 0x93, 0xFB, 0xF7, 0x40, 0x73, 0x28, 0x6E, 0x76,
            0xD5, 0xB1, 0x2D, 0x95, 0x70, 0xF4, 0x3C, 0x34, 0xE5, 0x4C, 0x5B, 0xBB, 0x5F, 0x50, 0x58, 0x8D,
            0x6B, 0xB7, 0x61, 0x09, 0xF2, 0x48, 0xCA, 0x81, 0x37, 0x45, 0xEF, 0xD0, 0xBE, 0xD9, 0xD4, 0xE7,
            0x9D, 0x33, 0x91, 0x71, 0x2F, 0x3B, 0xE6, 0x0D, 0xFE, 0x79, 0x49, 0x67, 0x19, 0xA5, 0x08, 0xAF,
            0x80, 0xB2, 0xEB, 0x3E, 0xD2, 0xB9, 0xD1, 0x44, 0x57, 0x8F, 0x8A, 0x4B, 0x39, 0xF1, 0x66, 0xEA,
            0xE2, 0xDF, 0xF3, 0x7A, 0x98, 0xCD, 0xAB, 0x8B, 0x04, 0x62, 0x54, 0x16, 0x12, 0x43, 0x02, 0xD8,
            0x36, 0x72, 0x06, 0x7F, 0x25, 0xE0, 0x2E, 0x05, 0x0F, 0xFF, 0xAD, 0x03, 0x07, 0xE1, 0x94, 0x17,
            0xC1, 0x32, 0xC3, 0x51, 0xD7, 0xDB, 0xE8, 0xE4, 0x75, 0x3F, 0x01, 0x26, 0x4A, 0x29, 0x64, 0x47,
            0x86, 0x3D, 0xBD, 0xDC, 0x83, 0x2B, 0x68, 0x1D, 0x46, 0xEC, 0xC4, 0x9A, 0xC8, 0x31, 0x4E, 0xA9,
            0xA4, 0x35, 0x9B, 0xAC, 0x5C, 0x0B, 0x92, 0xCC, 0x0A, 0x84, 0x13, 0x0C, 0x00, 0xA0, 0xB3, 0x60,
            0x18, 0x5A, 0xC5, 0xC6, 0x89, 0x7E, 0x21, 0xF9, 0xC2, 0x6D, 0xBC, 0xC7, 0xAE, 0x38, 0xFD, 0xF8
        };

        private RC4Crypto keyTable;
        private long position;
        public override long Position { get => position; set => throw new NotSupportedException(); }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => BaseStream.Length;
        public byte[] PasswordHash { get; }
        public long LengthOfUncryptedBlock { get; }
        public Stream BaseStream { get; }

        public CryptECReadStream(Stream stream, string password, long lengthOfUncryptedBlock, long initialPosition)
        {
            this.BaseStream = stream;
            byte[] passwordBytes = Encoding.GetEncoding("gbk").GetBytes(password);

            {
                byte[] passwordMd5 = MD5.Create().ComputeHash(passwordBytes);

                // 是的！该非标准MD5不是单纯的把标准MD5两两颠倒过来就好！
                // 以123为例
                // 标准MD5：202cb962ac59075b964b07152d234b70
                // 颠倒MD5：704b232d15074b965b0759ac62b92c20
                // 易式MD5：704b232d15074bb6590759ac62b92c20
                byte low4bit_7 = (byte)(passwordMd5[7] & 0x0F);
                byte high4bit_7 = (byte)(passwordMd5[7] & 0xF0);
                byte low4bit_8 = (byte)(passwordMd5[8] & 0x0F);
                byte high4bit_8 = (byte)(passwordMd5[8] & 0xF0);
                passwordMd5[7] = (byte)(high4bit_7 | high4bit_8 >> 4);
                passwordMd5[8] = (byte)(low4bit_7 << 4 | low4bit_8);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = passwordMd5.Length - 1; i >= 0; i--)
                {
                    stringBuilder.Append(passwordMd5[i].ToString("x2"));
                }
                this.PasswordHash = Encoding.ASCII.GetBytes(stringBuilder.ToString());
            }
            keyTable = new RC4Crypto(passwordBytes, initialStatus);
            this.LengthOfUncryptedBlock = lengthOfUncryptedBlock;
            this.position = initialPosition;
        }



        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long startPos = position;
            int lengthOfData = BaseStream.Read(buffer, 0, count);
            position += lengthOfData;

            long index = 0;
            long lengthOfCrypted = lengthOfData;
            long lengthOfDataInUncryptedBlock = LengthOfUncryptedBlock - startPos;
            if (lengthOfDataInUncryptedBlock > 0)
            {
                lengthOfCrypted -= lengthOfDataInUncryptedBlock;
                index += lengthOfDataInUncryptedBlock;
                startPos += lengthOfDataInUncryptedBlock;
            }

            if (lengthOfCrypted > 0)
            {
                byte[] tempKey = new byte[40];
                Array.Copy(PasswordHash, 0, tempKey, 8, 32);

                var tempKeyTable = (RC4Crypto)keyTable.Clone();
                tempKeyTable.Skip(8 * (startPos / 4096));

                int keyParamIndex = 0;
                byte[] keyParamBytes = new byte[((lengthOfCrypted / 4096) + 2) * 8];
                tempKeyTable.Decode(keyParamBytes, 0, keyParamBytes.Length);

                const int BlockLength = 4096;
                {
                    var lengthOfDataSkipInThisBlock = startPos % BlockLength;
                    if (lengthOfDataSkipInThisBlock >= 0)
                    {
                        Array.Copy(keyParamBytes, keyParamIndex, tempKey, 0, 8);
                        keyParamIndex += 8;
                        tempKeyTable = new RC4Crypto(tempKey, initialStatus);

                        tempKeyTable.Skip(lengthOfDataSkipInThisBlock);

                        var numOfDataInThisBlock = Math.Min(BlockLength - lengthOfDataSkipInThisBlock, lengthOfCrypted);
                        tempKeyTable.Decode(buffer, index, numOfDataInThisBlock);
                        index += numOfDataInThisBlock;
                        lengthOfCrypted -= numOfDataInThisBlock;
                    }
                }
                if (lengthOfCrypted > 0)
                {
                    while (true)
                    {
                        Array.Copy(keyParamBytes, keyParamIndex, tempKey, 0, 8);
                        keyParamIndex += 8;
                        tempKeyTable = new RC4Crypto(tempKey, initialStatus);
                        if (lengthOfCrypted <= BlockLength)
                        {
                            break;
                        }
                        tempKeyTable.Decode(buffer, index, BlockLength);
                        index += BlockLength;
                        lengthOfCrypted -= BlockLength;
                    }
                    if (lengthOfCrypted > 0)
                    {
                        tempKeyTable.Decode(buffer, index, lengthOfCrypted);
                        index += lengthOfCrypted;
                        lengthOfCrypted = 0;
                    }
                }
            }
            return lengthOfData;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            BaseStream.Dispose();
        }
    }

}
