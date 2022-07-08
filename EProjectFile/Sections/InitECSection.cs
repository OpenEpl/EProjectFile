using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Sections
{
    public class InitECSection : ISection
    {
        private class KeyImpl : ISectionKey<InitECSection>
        {
            public string SectionName => "初始模块段";
            public int SectionKey => 0x08007319;
            public bool IsOptional => false;

            public InitECSection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                var initEcSectionInfo = new InitECSection();
                using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
                {
                    initEcSectionInfo.EcName = reader.ReadStringsWithMfcStyleCountPrefix(encoding);
                    initEcSectionInfo.InitMethod = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4);
                }
                return initEcSectionInfo;
            }
        }

        public static readonly ISectionKey<InitECSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public string[] EcName { get; set; }
        public int[] InitMethod { get; set; }
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
