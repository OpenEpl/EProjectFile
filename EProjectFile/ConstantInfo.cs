using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class ConstantInfo : IHasId, IToTextCodeAble
    {
        private class ConstantValueConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Boolean:
                    case JsonToken.Float:
                    case JsonToken.Integer:
                    case JsonToken.String:
                        return reader.Value;
                }
                if (reader.TokenType != JsonToken.StartObject)
                {
                    throw new Exception();
                }
                object value = null;
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            if (value != null)
                            {
                                throw new Exception();
                            }
                            var keyName = (string)reader.Value;
                            reader.Read();
                            if ("bytes".Equals(keyName))
                            {
                                value = new HexConverter().ReadJson(reader, typeof(byte[]), null, serializer);
                            }
                            else if ("date".Equals(keyName))
                            {
                                value = new IsoDateTimeConverter().ReadJson(reader, typeof(DateTime), null, serializer);
                            }
                            else
                            {
                                throw new Exception();
                            }
                            break;
                        case JsonToken.EndObject:
                            return value;
                        case JsonToken.Comment:
                            break;
                        default:
                            throw new Exception();
                    }
                }
                throw new Exception();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                switch (value)
                {
                    case byte[] _:
                        writer.WriteStartObject();
                        writer.WritePropertyName("bytes");
                        new HexConverter().WriteJson(writer, value, serializer);
                        writer.WriteEndObject();
                        break;
                    case DateTime _:
                        writer.WriteStartObject();
                        writer.WritePropertyName("date");
                        new IsoDateTimeConverter().WriteJson(writer, value, serializer);
                        writer.WriteEndObject();
                        break;
                    default:
                        writer.WriteValue(value);
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
        public bool LongText { get => (Flags & 0x10) != 0; set => Flags = (Flags & ~0x10) | (value ? 0x10 : 0); }
        public string Name { get; set; }
        public string Comment { get; set; }
        [JsonConverter(typeof(ConstantValueConverter))]
        public object Value { get; set; } // 对于未验证代码，此值为string

        public static ConstantInfo[] ReadConstants(BinaryReader r, Encoding encoding)
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
        public static void WriteConstants(BinaryWriter w, Encoding encoding, ConstantInfo[] constants)
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
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
