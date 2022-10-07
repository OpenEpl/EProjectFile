using QIQI.EProjectFile.EditorTabInfo;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile.Internal
{
    internal class EditorTabInfoJsonConverter : JsonConverter<IEditorTabInfo>
    {
        public override IEditorTabInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var root = JsonNode.Parse(ref reader).AsObject();
            if (!root.TryGetPropertyValue(nameof(IEditorTabInfo.TypeId), out var typeIdNode))
            {
                throw new Exception($"Missing {nameof(IEditorTabInfo.TypeId)}");
            }
            var typeId = (byte)typeIdNode;
            if (PredefinedEditorTabInfos.Keys.TryGetValue(typeId, out var etik))
            {
                var editorTabInfoType = etik.GetType()
                    .GetInterfaces()
                    .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEditorTabInfoKey<>))
                    .GetGenericArguments()
                    .Single();
                return (IEditorTabInfo)JsonSerializer.Deserialize(root, editorTabInfoType, options);
            }
            else if (root.ContainsKey("Data"))
            {
                return JsonSerializer.Deserialize<GeneralEditorTabInfo>(root, options);
            }
            else
            {
                throw new Exception($"Failed to find a suitable JSON Deserializer of {nameof(IEditorTabInfo)} for Unknown[0x{typeId:X2}]");
            }
        }

        public override void Write(Utf8JsonWriter writer, IEditorTabInfo value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<object>(writer, value, options);
        }
    }
}
