using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class LosableSectionInfo
    {
        public const string SectionName = "可丢失程序段";
        public const int SectionKey = 0x05007319;
        public string OutFile { get; set; }
        public RemovedDefinedItemInfo[] RemovedDefinedItem { get; set; }
        [JsonIgnore]
        public byte[] UnknownAfterRemovedDefinedItem { get; set; }
        [Obsolete]
        public static LosableSectionInfo Parse(byte[] data) => Parse(data, Encoding.GetEncoding("gbk"));
        public static LosableSectionInfo Parse(byte[] data, Encoding encoding)
        {
            var losableSectionInfo = new LosableSectionInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
            {
                losableSectionInfo.OutFile = reader.ReadStringWithLengthPrefix(encoding);
                losableSectionInfo.RemovedDefinedItem = RemovedDefinedItemInfo.ReadRemovedDefinedItems(reader, encoding);
                losableSectionInfo.UnknownAfterRemovedDefinedItem = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }
            return losableSectionInfo;
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
