using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class InitEcSectionInfo
    {

        public const string SectionName = "初始模块段";
        public const int SectionKey = 0x08007319;
        public string[] EcName { get; set; }
        public int[] InitMethod { get; set; }
        [Obsolete]
        public static InitEcSectionInfo Parse(byte[] data) => Parse(data, Encoding.GetEncoding("gbk"));
        public static InitEcSectionInfo Parse(byte[] data, Encoding encoding)
        {
            var initEcSectionInfo = new InitEcSectionInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
            {
                initEcSectionInfo.EcName = reader.ReadStringsWithMfcStyleCountPrefix(encoding);
                initEcSectionInfo.InitMethod = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4);
            }
            return initEcSectionInfo;
        }
        [Obsolete]
        public byte[] ToBytes() => ToBytes(Encoding.GetEncoding("gbk"));
        public byte[] ToBytes(Encoding encoding)
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream(), encoding))
            {
                WriteTo(writer, encoding);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }
        private void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            writer.WriteStringsWithMfcStyleCountPrefix(encoding, EcName);
            writer.Write(InitMethod.Length * 4);
            writer.WriteInt32sWithoutLengthPrefix(InitMethod);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
