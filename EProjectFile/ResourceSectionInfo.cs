using System.IO;
using Newtonsoft.Json;

namespace QIQI.EProjectFile
{
    public class ResourceSectionInfo
    {
        public const string SectionName = "程序资源段";
        public FormInfo[] Forms;
        public ConstantInfo[] Constants;
        public static ResourceSectionInfo Parse(byte[] data)
        {
            ResourceSectionInfo resourceSectionInfo;
            using (var reader = new BinaryReader(new MemoryStream(data, false)))
            {
                resourceSectionInfo = new ResourceSectionInfo()
                {
                    Forms = FormInfo.ReadForms(reader),
                    Constants = ConstantInfo.ReadConstants(reader)
                };
            }
            return resourceSectionInfo;
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
            FormInfo.WriteForms(writer, Forms);
            ConstantInfo.WriteConstants(writer, Constants);
            writer.Write(0);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
