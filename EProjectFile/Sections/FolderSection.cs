using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class FolderSection : ISection
    {
        private class KeyImpl : ISectionKey<FolderSection>
        {
            public string SectionName => "编辑过滤器信息段";
            public int SectionKey => 0x0E007319;
            public bool IsOptional => true;

            public FolderSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    var folderSectionInfo = new FolderSection();
                    folderSectionInfo.allocatedKey = reader.ReadInt32();
                    while (!(reader.BaseStream.Position == reader.BaseStream.Length))
                    {
                        bool expand = reader.ReadInt32() != 0;
                        folderSectionInfo.Folders.Add(new CodeFolderInfo(reader.ReadInt32())
                        {
                            Expand = expand,
                            ParentKey = reader.ReadInt32(),
                            Name = reader.ReadStringWithLengthPrefix(encoding),
                            Children = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4)
                        });
                    }
                    return folderSectionInfo;
                });
            }
        }

        public static readonly ISectionKey<FolderSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        private int allocatedKey = 0;
        public List<CodeFolderInfo> Folders { get; set; } = new List<CodeFolderInfo>();

        public int AllocKey() => ++allocatedKey;

        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer => 
            {
                var encoding = context.Encoding;
                writer.Write(allocatedKey);
                foreach (var folder in Folders)
                {
                    writer.Write(folder.Expand ? 1 : 0);
                    writer.Write(folder.Key);
                    writer.Write(folder.ParentKey);
                    writer.WriteStringWithLengthPrefix(encoding, folder.Name);
                    writer.Write(folder.Children.Length * 4);
                    writer.WriteInt32sWithoutLengthPrefix(folder.Children);
                }
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
