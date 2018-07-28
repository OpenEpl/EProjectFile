using Newtonsoft.Json;
using System.IO;

namespace QIQI.EProjectFile
{
    public class InitEcSectionInfo
    {

        public const string SectionName = "初始模块段";
        public const int SectionKey = 0x08007319;
        public string[] EcName { get; set; }
        public int[] InitMethod { get; set; }
        public static InitEcSectionInfo Parse(byte[] data)
        {
            var initEcSectionInfo = new InitEcSectionInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false)))
            {
                initEcSectionInfo.EcName = reader.ReadStringsWithMfcStyleCountPrefix();
                initEcSectionInfo.InitMethod = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4);
            }
            return initEcSectionInfo;
        }

        public byte[] ToBytes()
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                WriteTo(writer);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }
        private void WriteTo(BinaryWriter writer)
        {
            writer.WriteStringsWithMfcStyleCountPrefix(EcName);
            writer.Write(InitMethod.Length * 4);
            writer.WriteInt32sWithoutLengthPrefix(InitMethod);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
