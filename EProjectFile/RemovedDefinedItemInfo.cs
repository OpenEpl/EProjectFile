using Newtonsoft.Json;
using QIQI.EProjectFile.Internal;
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
            var nameCode = reader.ReadInt32sWithFixedLength(count);
            var offsets = reader.ReadInt32sWithFixedLength(count);
            var startPosition = reader.BaseStream.Position;
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Position = startPosition + offsets[i];
                var name = reader.ReadCStyleString(encoding);
                result[i] = new RemovedDefinedItemInfo(ids[i])
                {
                    Name = name,
                };
                var calculatedNameCode = CalculateNameCode(name, encoding);
                if (nameCode[i] != calculatedNameCode)
                {
                    throw new Exception($"Invaild name code for \"{name}\" ({encoding.WebName}), "
                        + $"calculated 0x{calculatedNameCode:X8} but got 0x{nameCode[i]:X8}");
                }
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
            Array.ForEach(data, x => writer.Write(CalculateNameCode(x.Name, encoding)));
            writer.WriteInt32sWithoutLengthPrefix(offsets);
            Array.ForEach(elem, x => writer.Write(x));
        }

        private static int CalculateNameCode(string name, Encoding encoding)
        {
            if (string.IsNullOrEmpty(name))
            {
                return 0x01000000;
            }
            var bytes = encoding.GetBytes(name);
            byte composition1 = unchecked((byte)(bytes.Length + 1));
            ushort composition2 = 0;
            foreach (var item in bytes)
            {
                unchecked
                {
                    // 为了保持与易语言内部算法一致，不应当考虑双字节问题
                    // 否则形如以下编码的字符将无法被计算出正确结果
                    // 護 D76F (GB2312)
                    // 譸 D770 (GB2312)
                    if (item >= 'a' && item <= 'z')
                    {
                        composition2 -= 0x20;
                    }
                    composition2 += item;
                }
            }
            byte composition3 = bytes[0];
            return (composition1 << 24) | (composition2 << 8) | composition3;
        }
    }

}
