using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile
{
    public class ECDependencyInfo
    {
        public struct PackedIds
        {
            [JsonInclude]
            public int Start;
            [JsonInclude]
            public int Count;

            public override string ToString()
            {
                return JsonSerializer.Serialize(this, JsonUtils.Options);
            }
        }

        public int InfoVersion { get; set; }
        public int FileSize { get; set; }
        public DateTime FileLastModifiedDate { get; set; }
        public bool ReExport { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public List<PackedIds> DefinedIds { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
