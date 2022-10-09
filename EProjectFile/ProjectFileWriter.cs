using QIQI.EProjectFile.Encryption;
using QIQI.EProjectFile.Internal;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QIQI.EProjectFile
{
    public class ProjectFileWriter : IDisposable
    {
        private readonly BinaryWriter writer;
        public bool CryptEC { get; } = false;
        private int index;
        public ProjectFileWriter(Stream stream): this(stream, null)
        {

        }
        public ProjectFileWriter(Stream stream, EplEncryptionOptions encryptionOptions)
        {
            switch (encryptionOptions)
            {
                case EplEncryptionOptions.EStd eStd:
                    writer = new BinaryWriter(new CryptoStream(stream, new EStdCryptoTransform(eStd.Password, 8), CryptoStreamMode.Write));
                    writer.Write(0x454C5457); //WTLE
                    writer.Write(0x00000001);
                    writer.Write(eStd.Password.SecretId);
                    break;
                case EplEncryptionOptions.EC ec:
                    {
                        CryptEC = true;
                        var hint_bytes = Encoding.GetEncoding("gbk").GetBytes(ec.PasswordHint);
                        int lengthOfOvert = 4 /* [int]magic1 */ + 4 /* [int]magic2 */ + 4 /* [int]hint_length */ + hint_bytes.Length;
                        writer = new BinaryWriter(new CryptoStream(stream, new CryptoECTransform(ec.Password, lengthOfOvert), CryptoStreamMode.Write));
                        writer.Write(0x454C5457); //WTLE
                        writer.Write(0x00020001);
                        writer.WriteBytesWithLengthPrefix(hint_bytes);
                        writer.Write(ec.Password.SecretId);
                    }
                    break;
                case null:
                    writer = new BinaryWriter(stream);
                    break;
                default:
                    throw new ArgumentException("unsupported encryption is required", nameof(encryptionOptions));
            }
            writer.Write(0x4752504554574E43L); // CNWTEPRG
        }
        public void WriteSection(RawSectionInfo section)
        {
            index++;

            writer.Write(0x15117319); // Magic

            byte[] headerData;
            using (var headerWriter = new BinaryWriter(new MemoryStream()))
            {
                headerWriter.Write(section.Key);
                headerWriter.Write(EncodeName(section.Key, section.Name));
                headerWriter.Write(new byte[2]);
                headerWriter.Write(index); // 从1开始
                headerWriter.Write(section.IsOptional ? 1 : 0);
                headerWriter.Write(GetCheckSum(section.Data));
                headerWriter.Write(section.Data.Length);
                headerWriter.Write(new byte[40]);
                headerData = ((MemoryStream)headerWriter.BaseStream).ToArray();
            }

            writer.Write(GetCheckSum(headerData));
            if (CryptEC)
            {
                headerData[48] ^= 1; // mark it after calculating the checksum
            }
            writer.Write(headerData);
            writer.Write(section.Data);
        }

        private static byte[] EncodeName(int key, string name)
        {
            var r = new byte[30];
            if (name != null) 
            {
                Encoding.GetEncoding("gbk").GetBytes(name, 0, name.Length, r, 0);
            }
            if (key != 0x07007319)
            {
                var keyBytes = unchecked(new byte[] { (byte)key, (byte)(key >> 8), (byte)(key >> 16), (byte)(key >> 24) });
                for (int i = 0; i < r.Length; i++)
                {
                    r[i] ^= keyBytes[(i + 1) % 4];
                }
            }
            return r;
        }

        private static int GetCheckSum(byte[] data)
        {
            var checkSum = new byte[4];
            for (int i = 0; i < data.Length; i++)
            {
                checkSum[i & 0x3] ^= data[i];
            }
            return checkSum[3] << 24 | checkSum[2] << 16 | checkSum[1] << 8 | checkSum[0];
        }
        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    writer.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
