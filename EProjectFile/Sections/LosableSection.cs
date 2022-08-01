using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile.Sections
{
    public class LosableSection : ISection
    {
        private class KeyImpl : ISectionKey<LosableSection>
        {
            public string SectionName => "可丢失程序段";
            public int SectionKey => 0x05007319;
            public bool IsOptional => true;

            public LosableSection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                var losableSectionInfo = new LosableSection();
                using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
                {
                    losableSectionInfo.OutFile = reader.ReadStringWithLengthPrefix(encoding);
                    losableSectionInfo.RemovedDefinedItem = RemovedDefinedItemInfo.ReadRemovedDefinedItems(reader, encoding);
                    losableSectionInfo.UnknownAfterRemovedDefinedItem = reader.ReadImmutableBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)) switch
                    {
                        // use shared object if it equals to the dafault value, which can reduce memory usage.
                        var x when x.SequenceEqual(DefaultUnknownAfterRemovedDefinedItem) => DefaultUnknownAfterRemovedDefinedItem,
                        var x => x
                    };
                }
                return losableSectionInfo;
            }
        }
        public static readonly ISectionKey<LosableSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public string OutFile { get; set; }
        public RemovedDefinedItemInfo[] RemovedDefinedItem { get; set; }
        private static readonly ImmutableArray<byte> DefaultUnknownAfterRemovedDefinedItem 
            = ImmutableArray.Create(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255 } );
        [JsonIgnore]
        public ImmutableArray<byte> UnknownAfterRemovedDefinedItem { get; set; } = DefaultUnknownAfterRemovedDefinedItem;
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
            writer.WriteStringWithLengthPrefix(encoding, OutFile);
            RemovedDefinedItemInfo.WriteRemovedDefinedItems(writer, encoding, RemovedDefinedItem);
            writer.Write(UnknownAfterRemovedDefinedItem);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
