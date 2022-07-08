using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QIQI.EProjectFile
{
    public class ESystemInfo: ISectionInfo
    {
        private class KeyImpl : ISectionInfoKey<ESystemInfo>
        {
            public string SectionName => "系统信息段";
            public int SectionKey => 0x02007319;
            public bool IsOptional => false;

            public ESystemInfo Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                var systemInfo = new ESystemInfo();
                using (var reader = new BinaryReader(new MemoryStream(data, false)))
                {
                    systemInfo.ESystemVersion = new Version(reader.ReadInt16(), reader.ReadInt16());
                    reader.ReadInt32(); // Skip Unknown
                    systemInfo.Language = reader.ReadInt32();
                    systemInfo.EProjectFormatVersion = new Version(reader.ReadInt16(), reader.ReadInt16());
                    systemInfo.FileType = reader.ReadInt32();
                    reader.ReadInt32(); // Skip Unknown
                    systemInfo.ProjectType = reader.ReadInt32();
                }
                return systemInfo;
            }
        }
        public static readonly ISectionInfoKey<ESystemInfo> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        [JsonConverter(typeof(VersionConverter))]
        public Version ESystemVersion { get; set; }
        /// <summary>
        /// 1=中文（GBK），2=英语，3=中文（BIG5），4=日文（SJIS）
        /// </summary>
        public int Language { get; set; } = 1;

        public Encoding DetermineEncoding()
        {
            switch (Language)
            {
                case 2:
                    return Encoding.ASCII;
                case 3:
                    return Encoding.GetEncoding("big5");
                case 4:
                    return Encoding.GetEncoding("sjis");
                default:
                    return Encoding.GetEncoding("gbk");
            }
        }

        [JsonConverter(typeof(VersionConverter))]
        public Version EProjectFormatVersion { get; set; }
        /// <summary>
        /// 1=源码，3=模块
        /// </summary>
        public int FileType { get; set; } = 1;

        public int ProjectType { get; set; }

        public byte[] ToBytes(Encoding encoding)
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
            writer.Write((short)ESystemVersion.Major);
            writer.Write((short)ESystemVersion.Minor);
            writer.Write(1);
            writer.Write(Language);
            writer.Write((short)EProjectFormatVersion.Major);
            writer.Write((short)EProjectFormatVersion.Minor);
            writer.Write(FileType);
            writer.Write(0);
            writer.Write(ProjectType);
            writer.Write(new byte[32]);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}