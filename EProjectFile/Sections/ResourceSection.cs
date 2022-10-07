using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using QIQI.EProjectFile.Context;
using QIQI.EProjectFile.Internal;
using QIQI.EProjectFile.Sections;

namespace QIQI.EProjectFile
{
    public class ResourceSection : IToTextCodeAble, ISection
    {
        private class KeyImpl : ISectionKey<ResourceSection>
        {
            public string SectionName => "程序资源段";
            public int SectionKey => 0x04007319;
            public bool IsOptional => false;

            public ResourceSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    ResourceSection resourceSectionInfo;
                    resourceSectionInfo = new ResourceSection()
                    {
                        Forms = FormInfo.ReadForms(reader, encoding),
                        Constants = ConstantInfo.ReadConstants(reader, encoding)
                    };
                    return resourceSectionInfo;
                });
            }
        }

        public static readonly ISectionKey<ResourceSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public List<FormInfo> Forms { get; set; }
        public List<ConstantInfo> Constants { get; set; }
        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer => 
            {
                var encoding = context.Encoding;
                FormInfo.WriteForms(writer, encoding, Forms);
                ConstantInfo.WriteConstants(writer, encoding, Constants);
                writer.Write(0);
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }

        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            TextCodeUtils.JoinAndWriteCode(Constants, Environment.NewLine, nameMap, writer, indent);
        }
    }
}
