using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QIQI.EProjectFile.Internal;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class ProjectConfigExSection : ISection
    {
        private class KeyImpl : ISectionKey<ProjectConfigExSection>
        {
            public string SectionName => "辅助信息段3";
            public int SectionKey => 0x10007319;
            public bool IsOptional => true;

            public ProjectConfigExSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    var that = new ProjectConfigExSection();
                    that.ExternalFilePaths = reader.ReadStringsAsListWithMfcStyleCountPrefix(encoding);
                    that.ECPassword = reader.ReadStringWithLengthPrefix(encoding);
                    that.ECPasswordTips = reader.ReadStringWithLengthPrefix(encoding);
                    return that;
                });
            }
        }

        public static readonly ISectionKey<ProjectConfigExSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        /// <summary>
        /// 外部文件记录表
        /// </summary>
        public List<string> ExternalFilePaths { get; set; }
        /// <summary>
        /// 编译易模块时，加密模块所使用的密码，留空表示不加密。
        /// </summary>
        /// <remarks>设置在模块源码中。</remarks>
        public string ECPassword { get; set; }
        /// <summary>
        /// 编译易模块时，所输出加密模块的密码的提示文本。
        /// </summary>
        /// <remarks>设置在模块源码中。</remarks>
        public string ECPasswordTips { get; set; }
        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer => 
            {
                var encoding = context.Encoding;
                writer.WriteStringsWithMfcStyleCountPrefix(encoding, ExternalFilePaths);
                writer.WriteStringWithLengthPrefix(encoding, ECPassword);
                writer.WriteStringWithLengthPrefix(encoding, ECPasswordTips);
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
