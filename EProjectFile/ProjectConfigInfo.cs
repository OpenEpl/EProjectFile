using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class ProjectConfigInfo
    {
        public const string SectionName = "用户信息段";
        public const int SectionKey = 0x01007319;
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public string TelephoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string Email { get; set; }
        public string Homepage { get; set; }
        public string Copyright { get; set; }
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }
        public bool WriteVersion { get; set; }
        public string CompilePlugins { get; set; }
        public bool ExportPublicClassMethod { get; set; }
        [Obsolete]
        public static ProjectConfigInfo Parse(byte[] data) => Parse(data, Encoding.GetEncoding("gbk"));
        public static ProjectConfigInfo Parse(byte[] data, Encoding encoding)
        {
            var projectConfig = new ProjectConfigInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
            {
                projectConfig.Name = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.Description = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.Author = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.ZipCode = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.Address = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.TelephoneNumber = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.FaxNumber = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.Email = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.Homepage = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.Copyright = reader.ReadStringWithLengthPrefix(encoding);
                projectConfig.Version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                projectConfig.WriteVersion = reader.ReadInt32() == 0;
                projectConfig.CompilePlugins = reader.ReadStringWithFixedLength(encoding, 20);
                projectConfig.ExportPublicClassMethod = reader.ReadInt32() != 0;
                reader.ReadInt32(); // Unknown
            }
            return projectConfig;
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
            writer.WriteStringWithLengthPrefix(encoding, Name);
            writer.WriteStringWithLengthPrefix(encoding, Description);
            writer.WriteStringWithLengthPrefix(encoding, Author);
            writer.WriteStringWithLengthPrefix(encoding, ZipCode);
            writer.WriteStringWithLengthPrefix(encoding, Address);
            writer.WriteStringWithLengthPrefix(encoding, TelephoneNumber);
            writer.WriteStringWithLengthPrefix(encoding, FaxNumber);
            writer.WriteStringWithLengthPrefix(encoding, Email);
            writer.WriteStringWithLengthPrefix(encoding, Homepage);
            writer.WriteStringWithLengthPrefix(encoding, Copyright);
            writer.Write(Version.Major);
            writer.Write(Version.Minor);
            writer.Write(Version.Build);
            writer.Write(Version.Revision);
            writer.Write(WriteVersion ? 0 : 1);
            writer.WriteStringWithFixedLength(encoding, CompilePlugins, 20);
            writer.Write(ExportPublicClassMethod ? 1 : 0);
            writer.Write(0); // Unknown
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}