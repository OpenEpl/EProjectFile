using QIQI.EProjectFile.Context;
using QIQI.EProjectFile.Internal;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace QIQI.EProjectFile.Sections
{
    public class ESystemInfoSection : ISection
    {
        private class KeyImpl : ISectionKey<ESystemInfoSection>
        {
            public string SectionName => "系统信息段";
            public int SectionKey => 0x02007319;
            public bool IsOptional => false;

            public ESystemInfoSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var systemInfo = new ESystemInfoSection();
                    systemInfo.ESystemVersion = new Version(reader.ReadInt16(), reader.ReadInt16());
                    reader.ReadInt32(); // Skip Unknown
                    systemInfo.Language = reader.ReadInt32();
                    systemInfo.EProjectFormatVersion = new Version(reader.ReadInt16(), reader.ReadInt16());
                    systemInfo.FileType = reader.ReadInt32();
                    reader.ReadInt32(); // Skip Unknown
                    systemInfo.ProjectType = reader.ReadInt32();
                    return systemInfo;
                });
            }
        }
        public static readonly ISectionKey<ESystemInfoSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

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

        public Version EProjectFormatVersion { get; set; }
        /// <summary>
        /// 1=源码，3=模块
        /// </summary>
        public int FileType { get; set; } = 1;

        public int ProjectType { get; set; }

        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer => 
            {
                var encoding = context.Encoding;
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
            });
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}