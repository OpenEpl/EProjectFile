using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
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

            public ResourceSection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                ResourceSection resourceSectionInfo;
                using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
                {
                    resourceSectionInfo = new ResourceSection()
                    {
                        Forms = FormInfo.ReadForms(reader, encoding),
                        Constants = ConstantInfo.ReadConstants(reader, encoding)
                    };
                }
                return resourceSectionInfo;
            }
        }

        public static readonly ISectionKey<ResourceSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public FormInfo[] Forms { get; set; }
        public ConstantInfo[] Constants { get; set; }
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
            FormInfo.WriteForms(writer, encoding, Forms);
            ConstantInfo.WriteConstants(writer, encoding, Constants);
            writer.Write(0);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            TextCodeUtils.JoinAndWriteCode(Constants, Environment.NewLine, nameMap, writer, indent);
        }
    }
}
