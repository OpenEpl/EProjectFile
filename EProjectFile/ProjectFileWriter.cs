using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class ProjectFileWriter : IDisposable
    {
        private BinaryWriter writer;
        private int Index;
        public ProjectFileWriter(Stream stream)
        {
            writer = new BinaryWriter(stream, Encoding.GetEncoding("gbk"));
            writer.Write(0x4752504554574E43L);//CNWTEPRG
        }
        public void WriteSection(SectionInfo section)
        {
            Index++;

            writer.Write(0x15117319);//Magic

            byte[] headerData;
            using (var headerWriter = new BinaryWriter(new MemoryStream()))
            {
                headerWriter.Write(section.Key);
                headerWriter.Write(EncodeName(section.Key, section.Name));
                headerWriter.Write(new byte[2]);
                headerWriter.Write(Index);
                headerWriter.Write(section.CanSkip ? 1 : 0);
                headerWriter.Write(GetCheckSum(section.Data));
                headerWriter.Write(section.Data.Length);
                headerWriter.Write(new byte[40]);
                headerData = ((MemoryStream)headerWriter.BaseStream).ToArray();
            }

            writer.Write(GetCheckSum(headerData));
            writer.Write(headerData);
            writer.Write(section.Data);
        }

        private static byte[] EncodeName(byte[] key, string name)
        {
            var r = new byte[30];
            if (name != null) 
            {
                Encoding.GetEncoding("gbk").GetBytes(name, 0, name.Length, r, 0);
            }
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
