using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QIQI.EProjectFile.Internal;
using System.Linq;

namespace QIQI.EProjectFile.Sections
{
    public class ConditionalCompilationSection : ISection
    {
        private class KeyImpl : ISectionKey<ConditionalCompilationSection>
        {
            public string SectionName => "编译条件信息段";
            public int SectionKey => 0x11007319;
            public bool IsOptional => true;

            public ConditionalCompilationSection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                var that = new ConditionalCompilationSection();
                using (BinaryReader reader = new BinaryReader(new MemoryStream(data, false), encoding))
                {
                    that.ActivatedScheme = reader.ReadInt32();
                    var names = reader.ReadStringsWithMfcStyleCountPrefix(encoding);
                    var allFeatures = reader.ReadStringsWithMfcStyleCountPrefix(encoding);
                    that.Schemes = names.Zip(allFeatures, (name, features) => new CompilationSchemeInfo()
                    {
                        Name = name,
                        Features = features
                    }).ToList();
                }
                return that;
            }
        }

        public static readonly ISectionKey<ConditionalCompilationSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public int ActivatedScheme { get; set; } = -1;
        public List<CompilationSchemeInfo> Schemes { get; set; }

        public byte[] ToBytes(Encoding encoding)
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream(), encoding))
            {
                WriteTo(writer, encoding);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }
        private void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            writer.Write(ActivatedScheme);
            writer.WriteStringsWithMfcStyleCountPrefix(encoding, Schemes, x => x.Name);
            writer.WriteStringsWithMfcStyleCountPrefix(encoding, Schemes, x => x.Features);
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
