using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile.Internal
{
    internal class ByteArrayHexConverter : JsonConverter<byte[]>
    {
        public static byte[] HexToBytes(string src)
        {
            byte[] result = new byte[src.Length / 2];
            for (int i = 0, c = 0; i < src.Length; i += 2, c++)
            {
                result[c] = Convert.ToByte(src.Substring(i, 2), 16);
            }
            return result;
        }

        public static string BytesToHex(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 2);
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("X2"));
                }
            }
            return sb.ToString();
        }

        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return HexToBytes(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(BytesToHex(value));
        }
    }
}
