using QIQI.EProjectFile.Internal;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace QIQI.EProjectFile
{
    public class ProjectFileReader : IDisposable
    {
        public delegate string OnInputPassword(string tip);
        public bool IsFinish { get; private set; } = false;

        private BinaryReader reader;
        public bool CryptEC { get; } = false;

        public ProjectFileReader(Stream stream, OnInputPassword inputPassword = null)
        {
            reader = new BinaryReader(stream, Encoding.GetEncoding("gbk"));
            int magic1 = reader.ReadInt32();
            int magic2 = reader.ReadInt32();
            if (magic1 == 0x454C5457) // WTLE
            {
                if (magic2 != 0x00020001)
                {
                    throw new Exception("不支持此类加密文件");
                }
                CryptEC = true;
                int tip_bytes = reader.ReadInt32();
                string tip = reader.ReadStringWithFixedLength(Encoding.GetEncoding("gbk"), tip_bytes);
                string password = inputPassword?.Invoke(tip);
                if (string.IsNullOrEmpty(password))
                {
                    throw new Exception("没有输入密码 或 未正确响应InputPassword事件");
                }
                int lengthOfRead = 4 /* [int]magic1 */ + 4 /* [int]magic2 */ + 4 /* [int]tip_bytes */ + tip_bytes;
                var cryptoTransform = new CryptoECTransform(Encoding.GetEncoding("gbk").GetBytes(password));

                // 分块的时候，是不区分加密与非加密部分的，不进行 SeekToBegin 直接解密会导致分块错误
                // 然而我们不能保证 Stream 一定 CanSeek，因此使用 PrefixedStream
                var cryptoStream = new CryptoStream(new PrefixedStream(stream, new byte[lengthOfRead]), cryptoTransform, CryptoStreamMode.Read);

                // 跳过非加密部分（但使得 CryptoStream 按与加密部分相同的方式来将这些数据考虑进分块过程）
                cryptoStream.Read(new byte[lengthOfRead], 0, lengthOfRead);

                reader = new BinaryReader(cryptoStream);

                if (!reader.ReadBytes(32).SequenceEqual(cryptoTransform.PasswordHash)) 
                {
                    throw new Exception("密码错误"); 
                }

                magic1 = reader.ReadInt32();
                magic2 = reader.ReadInt32();
            }
            if (magic1 != 0x54574E43 || magic2 != 0x47525045) // CNWTEPRG
            {
                throw new Exception("不是易语言工程文件");
            }
        }

        public RawSectionInfo ReadSection()
        {
            if (IsFinish) 
            {
                throw new EndOfStreamException();
            }
            RawSectionInfo section = new RawSectionInfo();
            if (!(reader.ReadInt32() == 0x15117319))
            {
                throw new Exception("Magic错误");
            }
            reader.ReadInt32(); // Skip InfoCheckSum
            section.Key = reader.ReadInt32();
            section.Name = DecodeName(section.Key, reader.ReadBytes(30));
            reader.ReadInt16(); // 对齐填充（确认于易语言V5.71）
            reader.ReadInt32(); // Skip Index
            section.IsOptional = reader.ReadInt32() != 0;
            reader.ReadInt32(); // Skip DataCheckSum
            int dataLength = reader.ReadInt32();
            if (CryptEC)
            {
                dataLength ^= 1;
            }
            reader.ReadBytes(40); // 保留未用（确认于易语言V5.71）
            section.Data = new byte[dataLength];
            reader.Read(section.Data, 0, dataLength);
            if (section.Key == 0x07007319) 
            {
                IsFinish = true;
            }
            return section;
        }


        private static string DecodeName(int key, byte[] encodedName)
        {
            if (encodedName == null)
            {
                return string.Empty;
            }
            byte[] r = (byte[])encodedName.Clone();
            if (key != 0x07007319)
            {
                var keyBytes = unchecked(new byte[] { (byte)key, (byte)(key >> 8), (byte)(key >> 16), (byte)(key >> 24) });
                for (int i = 0; i < r.Length; i++)
                {
                    r[i] ^= keyBytes[(i + 1) % 4];
                }
            }

            int count = Array.IndexOf<byte>(r, 0);
            if (count != -1)
            {
                var t = new byte[count];
                Array.Copy(r, t, count);
                r = t;
            }

            return Encoding.GetEncoding("gbk").GetString(r);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    reader.Dispose();
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
