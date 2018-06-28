using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace QIQI.EProjectFile
{
    public class ProjectConfigInfo
    {
        public const string SectionName = "用户信息段";

        public string Name;
        public string Description;
        public string Author;
        public string ZipCode;
        public string Address;
        public string TelephoneNumber;
        public string FaxNumber;
        public string Email;
        public string Homepage;
        public string Copyright;
        [JsonConverter(typeof(VersionConverter))]
        public Version Version;
        public bool WriteVersion;
        public string CompilePlugins;
        public bool ExportPublicClassMethod;
        public static ProjectConfigInfo Parse(byte[] data)
        {
            var projectConfig = new ProjectConfigInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false)))
            {
                projectConfig.Name = reader.ReadStringWithLengthPrefix();
                projectConfig.Description = reader.ReadStringWithLengthPrefix();
                projectConfig.Author = reader.ReadStringWithLengthPrefix();
                projectConfig.ZipCode = reader.ReadStringWithLengthPrefix();
                projectConfig.Address = reader.ReadStringWithLengthPrefix();
                projectConfig.TelephoneNumber = reader.ReadStringWithLengthPrefix();
                projectConfig.FaxNumber = reader.ReadStringWithLengthPrefix();
                projectConfig.Email = reader.ReadStringWithLengthPrefix();
                projectConfig.Homepage = reader.ReadStringWithLengthPrefix();
                projectConfig.Copyright = reader.ReadStringWithLengthPrefix();
                projectConfig.Version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                projectConfig.WriteVersion = reader.ReadInt32() == 0;
                projectConfig.CompilePlugins = reader.ReadStringWithFixedLength(20);
                projectConfig.ExportPublicClassMethod = reader.ReadInt32() != 0;

            }
            return projectConfig;
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
            writer.WriteStringWithLengthPrefix(Name);
            writer.WriteStringWithLengthPrefix(Description);
            writer.WriteStringWithLengthPrefix(Author);
            writer.WriteStringWithLengthPrefix(ZipCode);
            writer.WriteStringWithLengthPrefix(Address);
            writer.WriteStringWithLengthPrefix(TelephoneNumber);
            writer.WriteStringWithLengthPrefix(FaxNumber);
            writer.WriteStringWithLengthPrefix(Email);
            writer.WriteStringWithLengthPrefix(Homepage);
            writer.WriteStringWithLengthPrefix(Copyright);
            writer.Write(Version.Major);
            writer.Write(Version.Minor);
            writer.Write(Version.Build);
            writer.Write(Version.Revision);
            writer.Write(WriteVersion ? 0 : 1);
            writer.WriteStringWithFixedLength(CompilePlugins, 20);
            writer.Write(ExportPublicClassMethod ? 1 : 0);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}