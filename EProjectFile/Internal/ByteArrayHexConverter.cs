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
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return BytesUtils.HexToBytes(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(BytesUtils.BytesToHex(value));
        }
    }
}
