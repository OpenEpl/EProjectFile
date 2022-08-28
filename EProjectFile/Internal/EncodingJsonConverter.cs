using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile.Internal
{
    internal class EncodingJsonConverter : JsonConverter<Encoding>
    {
        public override Encoding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Encoding.GetEncoding(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Encoding value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.WebName);
        }
    }
}
