using OpenEpl.ELibInfo;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace QIQI.EProjectFile
{
    [JsonConverter(typeof(FormElementInfoJsonConverter))]
    public abstract class FormElementInfo : IHasId
    {
        private const int DataType_Menu = 65539;

        private class FormElementInfoJsonConverter : JsonConverter<FormElementInfo>
        {
            public override FormElementInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var element = JsonElement.ParseValue(ref reader);
                if (!element.TryGetProperty("DataType", out var jsonElement))
                {
                    throw new Exception($"Missing {nameof(FormElementInfo)}.{nameof(FormElementInfo.DataType)}");
                }
                if (!jsonElement.TryGetInt32(out var dataType))
                {
                    throw new Exception($"Unsupported {nameof(FormElementInfo)}.{nameof(FormElementInfo.DataType)} = {jsonElement}");
                }
                return dataType switch
                {
                    DataType_Menu => element.Deserialize<FormMenuInfo>(options),
                    _ => element.Deserialize<FormControlInfo>(options),
                };
            }

            public override void Write(
                Utf8JsonWriter writer,
                FormElementInfo value,
                JsonSerializerOptions options)
            {
                switch (value)
                {
                    case FormMenuInfo menu:
                        JsonSerializer.Serialize(writer, menu, options);
                        break;
                    case FormControlInfo control:
                        JsonSerializer.Serialize(writer, control, options);
                        break;
                    default:
                        throw new Exception($"Unknown sub class of {nameof(FormElementInfo)}");
                }
            }
        }

        public abstract int Id { get; }
        public int DataType { get; set; }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public bool Disable { get; set; }

        public static List<FormElementInfo> ReadFormElements(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndOffest((reader, id, length) =>
            {
                var dataType = reader.ReadInt32();
                FormElementInfo elem;
                if (dataType == DataType_Menu)
                {
                    elem = FormMenuInfo.ReadWithoutDataType(r, encoding, id, length - 4);
                }
                else
                {
                    elem = FormControlInfo.ReadWithoutDataType(r, encoding, id, length - 4);
                }
                elem.DataType = dataType;
                return elem;
            });
        }
        public static void WriteFormElements(BinaryWriter w, Encoding encoding, List<FormElementInfo> formElements)
        {
            w.WriteBlocksWithIdAndOffest(
                encoding,
                formElements, 
                (writer, elem) =>
                {
                    elem.WriteWithoutId(writer, encoding);
                });
        }

        protected abstract void WriteWithoutId(BinaryWriter writer, Encoding encoding);
    }
}
