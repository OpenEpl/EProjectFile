using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class FolderSectionInfo
    {

        public const string SectionName = "编辑过滤器信息段";
        public const int SectionKey = 0x0E007319;

        private int allocatedKey = 0;
        public List<CodeFolderInfo> Folders { get; set; } = new List<CodeFolderInfo>();

        public int AllocKey() => ++allocatedKey;

        [Obsolete]
        public static FolderSectionInfo Parse(byte[] data) => Parse(data, Encoding.GetEncoding("gbk"));
        public static FolderSectionInfo Parse(byte[] data, Encoding encoding)
        {
            var folderSectionInfo = new FolderSectionInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
            {
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
            }
            return folderSectionInfo;
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
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
