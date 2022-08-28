using QIQI.EProjectFile.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static System.Collections.Specialized.BitVector32;

namespace QIQI.EProjectFile.Internal
{
    internal class SectionJsonConverter : JsonConverter<ISection>
    {
        public override ISection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var root = JsonNode.Parse(ref reader).AsObject();
            if (!root.TryGetPropertyValue(nameof(ISection.SectionKey), out var sectionKeyNode))
            {
                throw new Exception($"Missing {nameof(ISection.SectionKey)}");
            }
            var sectionKey = (int)sectionKeyNode;
            if (PredefinedSections.Keys.TryGetValue(sectionKey, out var sk))
            {
                var sectionType = sk.GetType()
                    .GetInterfaces()
                    .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISectionKey<>))
                    .GetGenericArguments()
                    .Single();
                return (ISection)JsonSerializer.Deserialize(root, sectionType, options);
            }
            else if (root.ContainsKey("Data"))
            {
                return JsonSerializer.Deserialize<GeneralSection>(root, options);
            }
            else
            {
                if (root.TryGetPropertyValue(nameof(ISection.SectionName), out var sectionNameNode))
                {
                    throw new Exception($"Failed to find a suitable JSON Deserializer for {sectionNameNode}[0x{sectionKey:X8}]");
                }
                else
                {
                    throw new Exception($"Failed to find a suitable JSON Deserializer for Unknown[0x{sectionKey:X8}]");
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, ISection value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<object>(writer, value, options);
        }
    }
}
