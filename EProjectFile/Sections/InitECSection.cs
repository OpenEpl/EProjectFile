using System.Text.Json;
using QIQI.EProjectFile.Internal;
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

            public InitECSection Parse(Context.BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    var that = new InitECSection();
                    that.ECName = reader.ReadStringsWithMfcStyleCountPrefix(encoding);
                    that.InitMethod = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4);
                    return that;
                });
            }
        }

        public static readonly ISectionKey<InitECSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public string[] ECName { get; set; }
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
            writer.WriteStringsWithMfcStyleCountPrefix(encoding, ECName);
            writer.Write(InitMethod.Length * 4);
            writer.WriteInt32sWithoutLengthPrefix(InitMethod);
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
