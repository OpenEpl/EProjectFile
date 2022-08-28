using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile.Internal
{
    internal class ImmutableByteArrayHexConverter : JsonConverter<ImmutableArray<byte>>
    {
        public static ImmutableArray<byte> HexToBytes(string src)
        {
            var data = ByteArrayHexConverter.HexToBytes(src);
            return Unsafe.As<byte[], ImmutableArray<byte>>(ref data);
        }

        public static string BytesToHex(ImmutableArray<byte> data)
        {
            return ByteArrayHexConverter.BytesToHex(Unsafe.As<ImmutableArray<byte>, byte[]>(ref data));
        }

        public override ImmutableArray<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return HexToBytes(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, ImmutableArray<byte> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(BytesToHex(value));
        }
    }
}
