using System.Text.Json;
using System.Collections.Generic;
using QIQI.EProjectFile.Internal;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class EventIndicesSection : ISection
    {
        private class KeyImpl : ISectionKey<EventIndicesSection>
        {
            public string SectionName => "辅助信息段1";
            public int SectionKey => 0x0A007319;
            public bool IsOptional => true;

            public EventIndicesSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var that = new EventIndicesSection();
                    var count = context.DataLength / 16;
                    var indices = new List<IndexedEventInfo>(count);
                    for (int i = 0; i < count; i++)
                    {
                        indices.Add(new IndexedEventInfo()
                        {
                            FormId = reader.ReadInt32(),
                            UnitId = reader.ReadInt32(),
                            EventId = reader.ReadInt32(),
                            MethodId = reader.ReadInt32()
                        });
                    }
                    that.Indices = indices;
                    return that;
                });
            }
        }

        public static readonly ISectionKey<EventIndicesSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public List<IndexedEventInfo> Indices { get; set; }
        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer =>
            {
                foreach (var x in Indices)
                {
                    writer.Write(x.FormId);
                    writer.Write(x.UnitId);
                    writer.Write(x.EventId);
                    writer.Write(x.MethodId);
                }
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
