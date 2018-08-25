using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class RemovedDefinedItemInfo : IHasId
    {
        public int Id { get; }
        public string Name { get; set; }
        [JsonIgnore]
        public int UnknownAfterId { get; set; }
        public RemovedDefinedItemInfo(int id)
        {
            this.Id = id;
        }

        public static RemovedDefinedItemInfo[] ReadRemovedDefinedItems(BinaryReader reader, Encoding encoding)
        {
            var count = reader.ReadInt32();
            var size = reader.ReadInt32();
            var endPosition = reader.BaseStream.Position + size;
            var result = new RemovedDefinedItemInfo[count];
            var ids = reader.ReadInt32sWithFixedLength(count);
            var unknownAfterIds = reader.ReadInt32sWithFixedLength(count);
            var offsets = reader.ReadInt32sWithFixedLength(count);
            var startPosition = reader.BaseStream.Position;
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Position = startPosition + offsets[i];
                result[i] = new RemovedDefinedItemInfo(ids[i])
                {
                    Name = reader.ReadCStyleString(encoding),
                    UnknownAfterId = unknownAfterIds[i]
                };
            }
            reader.BaseStream.Position = endPosition;
            return result;
        }

        public static void WriteRemovedDefinedItems(BinaryWriter writer, Encoding encoding, RemovedDefinedItemInfo[] data)
        {
            if (data == null)
            {
                writer.Write(0);
                writer.Write(0);
                return;
            }
            var count = data.Length;
            var elem = new byte[count][];
            for (int i = 0; i < count; i++)
            {
                var elemStream = new MemoryStream();
                var elemBytes = encoding.GetBytes(data[i].Name);
                elemStream.Write(elemBytes, 0, elemBytes.Length);
                elemStream.WriteByte(0);
                elem[i] = elemStream.ToArray();
            }

            var offsets = new int[count];
            if (count > 0)
            {
                offsets[0] = 0;
            }
            for (int i = 1; i < count; i++)
            {
                offsets[i] = offsets[i - 1] + elem[i - 1].Length;
            }
            writer.Write(count);
            writer.Write((count * 12) + elem.Sum(x => x.Length));
            Array.ForEach(data, x => writer.Write(x.Id));
            Array.ForEach(data, x => writer.Write(x.UnknownAfterId));
            writer.WriteInt32sWithoutLengthPrefix(offsets);
            Array.ForEach(elem, x => writer.Write(x));
        }
    }

}
