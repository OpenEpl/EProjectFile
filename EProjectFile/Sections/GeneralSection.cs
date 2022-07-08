using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace QIQI.EProjectFile.Sections
{
    public class GeneralSection : ISection
    {
        private RawSectionInfo raw;

        public GeneralSection(RawSectionInfo raw)
        {
            this.raw = raw;
        }

        public string SectionName => raw.Name;

        public int SectionKey => raw.Key;

        public bool IsOptional => raw.IsOptional;

        [JsonConverter(typeof(HexConverter))]
        public byte[] Data { get => raw.Data; set => raw.Data = value; }

        public byte[] ToBytes(Encoding encoding) => (byte[])raw.Data.Clone();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
