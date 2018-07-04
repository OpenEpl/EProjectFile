using System;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class ProjectFileReader : IDisposable
    {
        public delegate string OnInputPassword(string tip);
        public bool IsFinish => reader.BaseStream.Position == reader.BaseStream.Length;

        private BinaryReader reader;
        public bool CryptEc { get; } = false;

        public ProjectFileReader(Stream stream, OnInputPassword inputPassword = null)
        {
            reader = new BinaryReader(stream, Encoding.GetEncoding("gbk"));
            int magic1 = reader.ReadInt32();
            int magic2 = reader.ReadInt32();
            if (magic1 == 0x454C5457) //WTLE
            {
                if (magic2 != 0x00020001)
                {
                    throw new Exception("不支持此类加密文件");
                }
                string tip = reader.ReadStringWithLengthPrefix();
                string password = inputPassword?.Invoke(tip);
                if (string.IsNullOrEmpty(password))
                {
                    throw new Exception("没有输入密码 或 未正确响应InputPassword事件");
                }
                var cryptECReadStream = new CryptECReadStream(stream, password, stream.Position);
                reader = new BinaryReader(cryptECReadStream, Encoding.GetEncoding("gbk"));
                if(!reader.ReadBytes(32).SequenceEqual(cryptECReadStream.PasswordHash))
                {
                    throw new Exception("密码错误");
                }
                CryptEc = true;

                magic1 = reader.ReadInt32();
                magic2 = reader.ReadInt32();
            }
            if (magic1 != 0x54574E43 || magic2 != 0x47525045) //CNWTEPRG
            {
                throw new Exception("不是易语言工程文件");
            }
        }

        public SectionInfo ReadSection()
        {
            SectionInfo section = new SectionInfo();
            if (!(reader.ReadInt32() == 0x15117319))
            {
                throw new Exception("Magic错误");
            }
            reader.ReadInt32();//Skip InfoCheckSum
            section.Key = reader.ReadBytes(4);
            section.Name = DecodeName(section.Key, reader.ReadBytes(30));
            reader.ReadInt16();//对齐填充（确认于易语言V5.71）
            reader.ReadInt32();//Skip Index
            section.CanSkip = reader.ReadInt32() != 0;
            reader.ReadInt32();//Skip DataCheckSum
            int DataLength = reader.ReadInt32();
            if (CryptEc)
            {
                DataLength ^= 1;
            }
            reader.ReadBytes(40);//保留未用（确认于易语言V5.71）
            section.Data = new byte[DataLength];
            reader.Read(section.Data, 0, DataLength);
            return section;
        }
        private static string DecodeName(byte[] key, byte[] encodedName)
        {
            if (encodedName == null || key == null)
            {
                return string.Empty;
            }
            byte[] r = (byte[])encodedName.Clone();
            if (key.Length != 4)
            {
                throw new Exception($"{nameof(key)}应为4字节");
            }
            if (!(key[0] == 25 && key[1] == 115 && key[2] == 0 && key[3] == 7))
            {
                for (int i = 0; i < r.Length; i++)
                {
                    r[i] ^= key[(i + 1) & 0x3];
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
