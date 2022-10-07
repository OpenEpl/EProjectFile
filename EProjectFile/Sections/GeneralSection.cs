using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class GeneralSection : ISection
    {
        private RawSectionInfo raw;

        public GeneralSection(RawSectionInfo raw)
        {
            this.raw = raw;
        }

        [JsonConstructor]
        public GeneralSection(string sectionName, int sectionKey, bool isOptional, byte[] data)
        {
            this.raw = new RawSectionInfo()
            {
                Name = sectionName,
                Key = sectionKey,
                IsOptional = isOptional,
                Data = data
            };
        }

        public string SectionName => raw.Name;

        public int SectionKey => raw.Key;

        public bool IsOptional => raw.IsOptional;

        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] Data { get => raw.Data; set => raw.Data = value; }

        public byte[] ToBytes(BlockByteifierContext context) => (byte[])raw.Data.Clone();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
