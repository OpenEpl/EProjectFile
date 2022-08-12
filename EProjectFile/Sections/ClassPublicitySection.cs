using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Sections
{
    public class ClassPublicitySection : ISection
    {
        private class KeyImpl : ISectionKey<ClassPublicitySection>
        {
            public string SectionName => "辅助信息段2";
            public int SectionKey => 0x0B007319;
            public bool IsOptional => true;

            public ClassPublicitySection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                var that = new ClassPublicitySection();
                var count = data.Length / 8;
                var publicities = new List<ClassPublicityInfo>(count);
                using (BinaryReader reader = new BinaryReader(new MemoryStream(data, false), encoding))
                {
                    for (int i = 0; i < count; i++)
                    {
                        publicities.Add(new ClassPublicityInfo()
                        {
                            Class = reader.ReadInt32(),
                            Public = reader.ReadInt32() != 0
                        });
                    }
                }
                that.ClassPublicities = publicities;
                return that;
            }
        }

        public static readonly ISectionKey<ClassPublicitySection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public List<ClassPublicityInfo> ClassPublicities { get; set; }
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
            foreach (var publicity in ClassPublicities)
            {
                writer.Write(publicity.Class);
                writer.Write(publicity.Public ? 1 : 0);
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
