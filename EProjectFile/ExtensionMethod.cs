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
        /// <param name="encoding"></param>
        /// <param name="length">包括终止符（如果有）</param>
        /// <returns></returns>
        public static string ReadStringWithFixedLength(this BinaryReader reader, Encoding encoding, int length)
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
            return encoding.GetString(bytes);
        }
        public static string ReadBStr(this BinaryReader reader, Encoding encoding)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            var str = encoding.GetString(reader.ReadBytes(length - 1));
            reader.ReadByte();
            return str;
        }
        public static string ReadStringWithLengthPrefix(this BinaryReader reader, Encoding encoding)
        {
            return reader.ReadStringWithFixedLength(encoding, reader.ReadInt32());
        }
        public static string ReadCStyleString(this BinaryReader reader, Encoding encoding)
        {
            // 不依赖reader的编码设置

            var memoryStream = new MemoryStream();
            byte value;
            while ((value = reader.ReadByte()) != 0)
            {
                memoryStream.WriteByte(value);
            }
            return encoding.GetString(memoryStream.ToArray());
        }
        public static int ReadMfcStyleCountPrefix(this BinaryReader reader)
        {
            ushort count_16bit = reader.ReadUInt16();
            if (count_16bit != (ushort)0xFFFFU) 
            {
                return count_16bit;
            }
            return reader.ReadInt32();
        }

        public static string[] ReadStringsWithMfcStyleCountPrefix(this BinaryReader reader, Encoding encoding)
        {
            var result = new string[ReadMfcStyleCountPrefix(reader)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = reader.ReadStringWithLengthPrefix(encoding);
            }
            return result;
        }
        public static TElem[] ReadBlocksWithIdAndOffest<TElem>(
            this BinaryReader reader,
            Func<BinaryReader, int, TElem> readFunction)
        {
            return ReadBlocksWithIdAndOffest(reader, (elemReader, id, length) => readFunction(elemReader, id));
        }

        public static TElem[] ReadBlocksWithIdAndOffest<TElem>(
            this BinaryReader reader,
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

        public static void WriteBlocksWithIdAndOffest<TElem>(
            this BinaryWriter writer,
            Encoding encoding,
            TElem[] data,
            Action<BinaryWriter, TElem> writeAction)
            where TElem : IHasId
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
                using (var elemWriter = new BinaryWriter(new MemoryStream(), encoding))
                {
                    writeAction(elemWriter, data[i]);
                    elem[i] = ((MemoryStream)elemWriter.BaseStream).ToArray();
                }
                using (var elemWriter = new BinaryWriter(new MemoryStream(), encoding))
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
            writer.Write((count * 8) + elem.Sum(x => x.Length));
            Array.ForEach(data, x => writer.Write(x.Id));
            writer.WriteInt32sWithoutLengthPrefix(offsets);
            Array.ForEach(elem, x => writer.Write(x));
        }

        public static TElem[] ReadBlocksWithIdAndMemoryAddress<TElem>(
            this BinaryReader reader,
            Func<BinaryReader, int, int, TElem> readFunction)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var memoryAddresses = reader.ReadInt32sWithFixedLength(count);
            TElem[] result = new TElem[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = readFunction(reader, ids[i], memoryAddresses[i]);
            }
            return result;
        }

        public static void WriteBlocksWithIdAndMemoryAddress<TElem>(
            this BinaryWriter writer,
            TElem[] data,
            Action<BinaryWriter, TElem> writeAction)
            where TElem : IHasId, IHasMemoryAddress
        {
            writer.Write(data.Length * 8);
            Array.ForEach(data, x => writer.Write(x.Id));
            Array.ForEach(data, x => writer.Write(x.MemoryAddress));
            Array.ForEach(data, x => writeAction(writer, x));
        }

        public static int[] ReadInt32sWithFixedLength(this BinaryReader reader, int count)
        {
            var result = new int[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = reader.ReadInt32();
            }
            return result;
        }
        public static int[] ReadInt32sWithLengthPrefix(this BinaryReader reader)
        {
            return reader.ReadInt32sWithFixedLength(reader.ReadInt32());
        }
        public static int[] ReadInt32sWithByteSizePrefix(this BinaryReader reader)
        {
            return reader.ReadInt32sWithFixedLength(reader.ReadInt32() / sizeof(int));
        }

        public static short[] ReadInt16sWithFixedLength(this BinaryReader reader, int count)
        {
            var result = new short[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = reader.ReadInt16();
            }
            return result;
        }
        public static short[] ReadInt16sWithLengthPrefix(this BinaryReader reader)
        {
            return reader.ReadInt16sWithFixedLength(reader.ReadInt32());
        }
        public static short[] ReadInt16sWithByteSizePrefix(this BinaryReader reader)
        {
            return reader.ReadInt16sWithFixedLength(reader.ReadInt32() / sizeof(short));
        }

        public static void WriteBytesWithLengthPrefix(this BinaryWriter writer, byte[] data)
        {
            if (data == null)
            {
                writer.Write(0);
                return;
            }
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
            if (data == null)
            {
                writer.Write(0);
                return;
            }
            writer.Write(data.Length);
            writer.WriteInt32sWithoutLengthPrefix(data);
        }
        public static void WriteInt32sWithByteSizePrefix(this BinaryWriter writer, int[] data)
        {
            if (data == null)
            {
                writer.Write(0);
                return;
            }
            writer.Write(data.Length * sizeof(int));
            writer.WriteInt32sWithoutLengthPrefix(data);
        }
        public static void WriteInt16sWithoutLengthPrefix(this BinaryWriter writer, short[] data)
        {
            if (data == null) return;
            Array.ForEach(data, x => writer.Write(x));
        }
        public static void WriteInt16sWithLengthPrefix(this BinaryWriter writer, short[] data)
        {
            if (data == null)
            {
                writer.Write(0);
                return;
            }
            writer.Write(data.Length);
            writer.WriteInt16sWithoutLengthPrefix(data);
        }
        public static void WriteInt16sWithByteSizePrefix(this BinaryWriter writer, short[] data)
        {
            if (data == null)
            {
                writer.Write(0);
                return;
            }
            writer.Write(data.Length * sizeof(short));
            writer.WriteInt16sWithoutLengthPrefix(data);
        }
        public static void WriteStringWithFixedLength(this BinaryWriter writer, Encoding encoding, string data, int length)
        {
            var bytes = encoding.GetBytes(data);
            if (bytes.Length > length)
                throw new ArgumentException("字符串过长");
            writer.Write(bytes);
            writer.Write(new byte[length - bytes.Length]);
        }
        public static void WriteBStr(this BinaryWriter writer, Encoding encoding, string data)
        {
            if (data == null)
            {
                writer.Write(0);
                return;
            }
            var bytes = encoding.GetBytes(data);
            writer.Write(bytes.Length + 1);
            writer.Write(bytes);
            writer.Write((byte)0);
        }
        public static void WriteStringWithLengthPrefix(this BinaryWriter writer, Encoding encoding, string data)
        {
            if (data == null)
            {
                writer.Write(0);
                return;
            }
            var bytes = encoding.GetBytes(data);
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
        public static void WriteStringsWithMfcStyleCountPrefix(this BinaryWriter writer, Encoding encoding, string[] data)
        {
            if (data == null)
            {
                writer.WriteMfcStyleCountPrefix(0);
                return;
            }
            writer.WriteMfcStyleCountPrefix(data.Length);
            Array.ForEach(data, x => writer.WriteStringWithLengthPrefix(encoding, x));
        }
        public static void WriteCStyleString(this BinaryWriter writer, Encoding encoding, string data)
        {
            if (data == null) data = string.Empty;
            writer.Write(encoding.GetBytes(data));
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
