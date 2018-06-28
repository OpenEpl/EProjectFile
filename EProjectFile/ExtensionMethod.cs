using System;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    internal static class ExtensionMethod
    {
        public static byte[] ReadBytesWithLengthPrefix(this BinaryReader reader)
        {
            return reader.ReadBytes(reader.ReadInt32());
        }
        /// <summary>
        /// 读取固定长度的文本
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="length">包括终止符（如果有）</param>
        /// <returns></returns>
        public static string ReadStringWithFixedLength(this BinaryReader reader,int length)
        {
            var bytes = reader.ReadBytes(length);
            {
                int count = Array.IndexOf<byte>(bytes, 0);
                if (count != -1)
                {
                    var t = new byte[count];
                    Array.Copy(bytes, t, count);
                    bytes = t;
                }
            }
            return Encoding.GetEncoding("gbk").GetString(bytes);
        }
        public static string ReadBStr(this BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            var str = Encoding.GetEncoding("gbk").GetString(reader.ReadBytes(length - 1));
            reader.ReadByte();
            return str;
        }
        public static string ReadStringWithLengthPrefix(this BinaryReader reader)
        {
            return reader.ReadStringWithFixedLength(reader.ReadInt32());
        }
        public static string ReadCStyleString(this BinaryReader reader)
        {
            //不依赖reader的编码设置

            var memoryStream = new MemoryStream();
            byte value;
            while ((value = reader.ReadByte()) != 0)
            {
                memoryStream.WriteByte(value);
            }
            return Encoding.GetEncoding("gbk").GetString(memoryStream.ToArray());
        }
        public static int ReadMfcStyleCountPrefix(this BinaryReader reader)
        {
            ushort _16bit = reader.ReadUInt16();
            if (_16bit != (ushort)0xFFFFU) 
            {
                return _16bit;
            }
            return reader.ReadInt32();
        }

        public static string[] ReadStringsWithMfcStyleCountPrefix(this BinaryReader reader)
        {
            return new object[ReadMfcStyleCountPrefix(reader)].Select(x => reader.ReadStringWithLengthPrefix()).ToArray();
        }
        public static TElem[] ReadBlocksWithIdAndOffest<TElem>(this BinaryReader reader,
            Func<BinaryReader, int, TElem> readFunction)
        {
            return ReadBlocksWithIdAndOffest(reader, (elemReader, id, length) => readFunction(elemReader, id));
        }

        public static TElem[] ReadBlocksWithIdAndOffest<TElem>(this BinaryReader reader,
            Func<BinaryReader, int, int, TElem> readFunction)
        {
            var count = reader.ReadInt32();
            var size = reader.ReadInt32();
            var endPosition = reader.BaseStream.Position + size;
            TElem[] result = new TElem[count];
            var ids = reader.ReadInt32sWithFixedLength(count);
            var offsets = reader.ReadInt32sWithFixedLength(count);
            var startPosition = reader.BaseStream.Position;
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Position = startPosition + offsets[i];
                int length = reader.ReadInt32();
                result[i] = readFunction(reader, ids[i], length);
            }
            reader.BaseStream.Position = endPosition;
            return result;
        }

        public static void WriteBlocksWithIdAndOffest<TElem> (this BinaryWriter writer,
            TElem[] data,
            Action<BinaryWriter, TElem> writeAction)
            where TElem:IHasId
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
                using (var elemWriter = new BinaryWriter(new MemoryStream()))
                {
                    writeAction(elemWriter, data[i]);
                    elem[i] = ((MemoryStream)elemWriter.BaseStream).ToArray();
                }
                using (var elemWriter = new BinaryWriter(new MemoryStream()))
                {
                    elemWriter.Write(elem[i].Length);
                    elemWriter.Write(elem[i]);
                    elem[i] = ((MemoryStream)elemWriter.BaseStream).ToArray();
                }
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
            writer.Write(count * 8 + elem.Sum(x => x.Length));
            Array.ForEach(data, x => writer.Write(x.Id));
            writer.WriteInt32sWithoutLengthPrefix(offsets);
            Array.ForEach(elem, x => writer.Write(x));
        }

        public static int[] ReadInt32sWithFixedLength(this BinaryReader reader, int count)
        {
            return new object[count].Select(x => reader.ReadInt32()).ToArray();
        }
        public static int[] ReadInt32sWithLengthPrefix(this BinaryReader reader)
        {
            return reader.ReadInt32sWithFixedLength(reader.ReadInt32());
        }

        public static void WriteBytesWithLengthPrefix(this BinaryWriter writer, byte[] data)
        {
            if (data == null) data = new byte[] { };
            writer.Write(data.Length);
            writer.Write(data);
        }
        public static void WriteInt32sWithoutLengthPrefix(this BinaryWriter writer, int[] data)
        {
            if (data == null) return;
            Array.ForEach(data, x => writer.Write(x));
        }
        public static void WriteInt32sWithLengthPrefix(this BinaryWriter writer, int[] data)
        {
            if (data == null) data = new int[] { };
            writer.Write(data.Length);
            writer.WriteInt32sWithoutLengthPrefix(data);
        }
        public static void WriteStringWithFixedLength(this BinaryWriter writer, string data, int length)
        {
            var bytes = Encoding.GetEncoding("gbk").GetBytes(data);
            if (bytes.Length > length)
                throw new ArgumentException("字符串过长");
            writer.Write(bytes);
            writer.Write(new byte[length - bytes.Length]);
        }
        public static void WriteBStr(this BinaryWriter writer, string data)
        {
            if (data == null)
            {
                writer.Write(0);
                return;
            }
            var bytes = Encoding.GetEncoding("gbk").GetBytes(data);
            writer.Write(bytes.Length + 1);
            writer.Write(bytes);
            writer.Write((byte)0);
        }
        public static void WriteStringWithLengthPrefix(this BinaryWriter writer, string data)
        {
            if (data == null)
            {
                writer.Write(0);
                return;
            }
            var bytes = Encoding.GetEncoding("gbk").GetBytes(data);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
        public static void WriteMfcStyleCountPrefix(this BinaryWriter writer, int data)
        {
            if (data < 0xFFFF)
            {
                writer.Write((ushort)data);
            }
            else
            {
                writer.Write((ushort)0xFFFFU);
                writer.Write(data);
            }
        }
        public static void WriteStringsWithMfcStyleCountPrefix(this BinaryWriter writer, string[] data)
        {
            if (data == null) data = new string[]{ };
            writer.WriteMfcStyleCountPrefix(data.Length);
            Array.ForEach(data, x => writer.WriteStringWithLengthPrefix(x));
        }
        public static void WriteCStyleString(this BinaryWriter writer, string data)
        {
            if (data == null) data = string.Empty;
            writer.Write(Encoding.GetEncoding("gbk").GetBytes(data));
            writer.Write((byte)0);
        }
        public static string ToHexString(this byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("X2"));
                }
            }
            return sb.ToString();
        }
    }
}
