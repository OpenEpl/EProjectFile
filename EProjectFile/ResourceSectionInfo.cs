using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace QIQI.EProjectFile
{
    public class ResourceSectionInfo : IToTextCodeAble
    {
        public const string SectionName = "程序资源段";
        public const int SectionKey = 0x04007319;
        public FormInfo[] Forms { get; set; }
        public ConstantInfo[] Constants { get; set; }
        [Obsolete]
        public static ResourceSectionInfo Parse(byte[] data) => Parse(data, Encoding.GetEncoding("gbk"));
        public static ResourceSectionInfo Parse(byte[] data, Encoding encoding)
        {
            ResourceSectionInfo resourceSectionInfo;
            using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
            {
                resourceSectionInfo = new ResourceSectionInfo()
                {
                    Forms = FormInfo.ReadForms(reader, encoding),
                    Constants = ConstantInfo.ReadConstants(reader, encoding)
                };
            }
            return resourceSectionInfo;
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
            FormInfo.WriteForms(writer, encoding, Forms);
            ConstantInfo.WriteConstants(writer, encoding, Constants);
            writer.Write(0);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            TextCodeUtils.WriteJoinCode(Constants, Environment.NewLine, nameMap, result, indent);
        }
    }
}
