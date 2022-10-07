using QIQI.EProjectFile.Context;
using QIQI.EProjectFile.EditorTabInfo;
using QIQI.EProjectFile.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace QIQI.EProjectFile.Sections
{
    public class EditorInfoSection : ISection
    {
        private class KeyImpl : ISectionKey<EditorInfoSection>
        {
            public string SectionName => "编辑信息段2";
            public int SectionKey => 0x09007319;
            public bool IsOptional => true;

            public EditorInfoSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    var that = new EditorInfoSection();
                    var count = reader.ReadInt32() + 1;
                    that.Tabs = new List<IEditorTabInfo>(count);
                    for (int i = 0; i < count; i++)
                    {
                        var itemData = reader.ReadBytesWithLengthPrefix();
                        if (itemData.Length == 0)
                        {
                            that.Tabs.Add(null);
                        }
                        var typeId = itemData[0];
                        if (PredefinedEditorTabInfos.Keys.TryGetValue(typeId, out var key))
                        {
                            that.Tabs.Add(key.Parse(new BlockParserContext(itemData, encoding, context.CryptEC)));
                        }
                        else
                        {
                            that.Tabs.Add(new GeneralEditorTabInfo(typeId, itemData.Skip(1).ToArray()));
                        }
                    }
                    return that;
                });
            }
        }
        public static readonly ISectionKey<EditorInfoSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public List<IEditorTabInfo> Tabs { get; set; }

        public byte[] ToBytes(Encoding encoding)
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                WriteTo(writer, encoding);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }

        private void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            if (Tabs is null)
            {
                writer.Write(-1);
                return;
            }
            writer.Write(Tabs.Count - 1);
            foreach (var tab in Tabs)
            {
                if (tab is null)
                {
                    writer.Write(0);
                    continue;
                }
                tab.WriteTo(writer, encoding);
            }
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
