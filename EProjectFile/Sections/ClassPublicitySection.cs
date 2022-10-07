using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QIQI.EProjectFile.Internal;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class ClassPublicitySection : ISection
    {
        private class KeyImpl : ISectionKey<ClassPublicitySection>
        {
            public string SectionName => "辅助信息段2";
            public int SectionKey => 0x0B007319;
            public bool IsOptional => true;

            public ClassPublicitySection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var that = new ClassPublicitySection();
                    var count = context.DataLength / 8;
                    var publicities = new List<ClassPublicityInfo>(count);
                    for (int i = 0; i < count; i++)
                    {
                        publicities.Add(new ClassPublicityInfo()
                        {
                            Class = reader.ReadInt32(),
                            Flags = reader.ReadInt32()
                        });
                    }
                    that.ClassPublicities = publicities;
                    return that;
                });
            }
        }

        public static readonly ISectionKey<ClassPublicitySection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public List<ClassPublicityInfo> ClassPublicities { get; set; }
        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer => 
            {
                var encoding = context.Encoding;
                foreach (var publicity in ClassPublicities)
                {
                    writer.Write(publicity.Class);
                    writer.Write(publicity.Flags);
                }
            });
        }
        private void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            foreach (var publicity in ClassPublicities)
            {
                writer.Write(publicity.Class);
                writer.Write(publicity.Flags);
            }
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
