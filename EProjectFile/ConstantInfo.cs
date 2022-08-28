using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile
{
    public class ConstantInfo : IHasId, IToTextCodeAble
    {
        private class ConstantValueConverter : JsonConverter<object>
        {
            public override bool HandleNull => true;

            public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.True:
                        return true;
                    case JsonTokenType.False:
                        return false;
                    case JsonTokenType.Number:
                        return reader.GetDouble();
                    case JsonTokenType.String:
                        return reader.GetString();
                    case JsonTokenType.Null:
                        return null;
                }
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new Exception("Unsupported constant value");
                }
                object value = null;
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.PropertyName:
                            if (value != null)
                            {
                                throw new Exception();
                            }
                            var keyName = reader.GetString();
                            reader.Read();
                            if ("bytes".Equals(keyName))
                            {
                                value = ByteArrayHexConverter.HexToBytes(reader.GetString());
                            }
                            else if ("date".Equals(keyName))
                            {
                                value = reader.GetDateTime();
                            }
                            else
                            {
                                throw new Exception($"Unsupported constant type: {keyName}");
                            }
                            break;
                        case JsonTokenType.EndObject:
                            return value;
                        case JsonTokenType.Comment:
                            break;
                        default:
                            throw new Exception($"Unknown token type while parsing constant value: {reader.TokenType}");
                    }
                }
                throw new Exception($"Unexpected EOF while parsing constant value");
            }

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                switch (value)
                {
                    case null:
                        writer.WriteNullValue();
                        break;
                    case byte[] bytes:
                        writer.WriteStartObject();
                        writer.WriteString("bytes", ByteArrayHexConverter.BytesToHex(bytes));
                        writer.WriteEndObject();
                        break;
                    case DateTime dateTime:
                        writer.WriteStartObject();
                        writer.WriteString("date", dateTime);
                        writer.WriteEndObject();
                        break;
                    default:
                        JsonSerializer.Serialize<object>(writer, value, options);
                        break;
                }
            }
        }
        public int Id { get; }

        public ConstantInfo(int id)
        {
            this.Id = id;
        }
        public int Flags { get; set; }
        public bool Unexamined { get => (Flags & 0x1) != 0; set => Flags = (Flags & ~0x1) | (value ? 0x1 : 0); }
        public bool Public { get => (Flags & 0x2) != 0; set => Flags = (Flags & ~0x2) | (value ? 0x2 : 0); }
        public bool Hidden { get => (Flags & 0x4) != 0; set => Flags = (Flags & ~0x4) | (value ? 0x4 : 0); }
        public bool LongText { get => (Flags & 0x10) != 0; set => Flags = (Flags & ~0x10) | (value ? 0x10 : 0); }
        public string Name { get; set; }
        public string Comment { get; set; }
        [JsonConverter(typeof(ConstantValueConverter))]
        public object Value { get; set; } // 对于未验证代码，此值为string

        public static List<ConstantInfo> ReadConstants(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndOffest(
                (reader, id) =>
                {
                    var constant = new ConstantInfo(id)
                    {
                        Flags = reader.ReadInt16(),
                        Name = reader.ReadCStyleString(encoding),
                        Comment = reader.ReadCStyleString(encoding)
                    };
                    switch (unchecked((uint)id) >> 28)
                    {
                        case 1: // 常量
                            byte type = reader.ReadByte();
                            switch (type)
                            {
                                case 22:
                                    constant.Value = null;
                                    break;
                                case 23:
                                    constant.Value = reader.ReadDouble();
                                    break;
                                case 24:
                                    constant.Value = reader.ReadInt32() != 0;
                                    break;
                                case 25:
                                    constant.Value = DateTime.FromOADate(reader.ReadDouble());
                                    break;
                                case 26:
                                    constant.Value = reader.ReadBStr(encoding);
                                    break;
                                default:
                                    throw new Exception();
                            }
                            break;
                        case 2: // 图片
                        case 3: // 声音
                            constant.Value = reader.ReadBytesWithLengthPrefix();
                            break;
                        default:
                            throw new Exception();
                    }
                    return constant;
                });
        }
        public static void WriteConstants(BinaryWriter w, Encoding encoding, List<ConstantInfo> constants)
        {
            w.WriteBlocksWithIdAndOffest(
                encoding,
                constants,
                (writer, elem) =>
                {
                    writer.Write((short)elem.Flags);
                    writer.WriteCStyleString(encoding, elem.Name);
                    writer.WriteCStyleString(encoding, elem.Comment);
                    switch (elem.Value)
                    {
                        case null:
                            writer.Write((byte)22);
                            break;
                        case byte[] v:
                            writer.WriteBytesWithLengthPrefix(v);
                            break;
                        case double v:
                            writer.Write((byte)23);
                            writer.Write(v);
                            break;
                        case bool v:
                            writer.Write((byte)24);
                            writer.Write(v ? 1 : 0);
                            break;
                        case DateTime v:
                            writer.Write((byte)25);
                            writer.Write(v.ToOADate());
                            break;
                        case string v:
                            writer.Write((byte)26);
                            writer.WriteBStr(encoding, v);
                            break;
                        default:
                            throw new Exception();
                    }
                });
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            string valueCode;
            switch (Value)
            {
                case null:
                    valueCode = "";
                    break;
                case string str:
                    if (LongText) 
                        valueCode = $"\"<文本长度: {Encoding.GetEncoding("gbk").GetBytes(str).Length}>\"";
                    else
                        valueCode = $"\"“{str}”\"";
                    break;
                case byte[] bytes:
                    valueCode = $"\"<资源: {Convert.ToBase64String(bytes)}>\"";
                    break;
                case bool boolValue:
                    valueCode = "\"" + (boolValue ? "真" : "假") + "\"";
                    break;
                case DateTime dateTime:
                    if (dateTime.TimeOfDay.TotalSeconds == 0)
                        valueCode = dateTime.ToString("\"[yyyy年MM月dd日]\"");
                    else
                        valueCode = dateTime.ToString("\"[yyyy年MM月dd日HH时mm分ss秒]\"");
                    break;
                default:
                    valueCode = $"\"{Value}\"";
                    break;
            }
            TextCodeUtils.WriteDefinitionCode(writer, 0, "常量", nameMap.GetUserDefinedName(Id), valueCode, Public ? "公开" : "", Comment);
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
