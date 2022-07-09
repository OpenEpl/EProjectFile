using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace QIQI.EProjectFile.Sections
{
    public class ECDependenciesSection : ISection
    {
        private class KeyImpl : ISectionKey<ECDependenciesSection>
        {
            public string SectionName => "易模块记录段";
            public int SectionKey => 0x0C007319;
            public bool IsOptional => false;

            public ECDependenciesSection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                var that = new ECDependenciesSection();
                using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
                {
                    var count = reader.ReadInt32();
                    that.ECDependencies = new List<ECDependencyInfo>(count);
                    for (int i = 0; i < count; i++)
                    {
                        var dependency = new ECDependencyInfo();
                        dependency.InfoVersion = reader.ReadInt32();
                        if (dependency.InfoVersion > 2)
                        {
                            throw new Exception($"{nameof(ECDependencyInfo)}.{nameof(ECDependencyInfo.InfoVersion)} = {dependency.InfoVersion}, not supported");
                        }
                        dependency.FileSize = reader.ReadInt32();
                        dependency.FileLastModifiedDate = DateTime.FromFileTime(reader.ReadInt64());
                        if (dependency.InfoVersion >= 2)
                        {
                            dependency.ReExport = reader.ReadInt32() != 0;
                        }
                        dependency.Name = reader.ReadStringWithLengthPrefix(encoding);
                        dependency.Path = reader.ReadStringWithLengthPrefix(encoding);
                        var starts = reader.ReadInt32sWithByteSizePrefix();
                        var counts = reader.ReadInt32sWithByteSizePrefix();
                        dependency.DefinedIds = new List<ECDependencyInfo.PackedIds>(starts.Length);
                        for (int indexOfids = 0; indexOfids < starts.Length; indexOfids++)
                        {
                            dependency.DefinedIds.Add(new ECDependencyInfo.PackedIds
                            {
                                Start = starts[indexOfids],
                                Count = counts[indexOfids]
                            });
                        }
                        that.ECDependencies.Add(dependency);
                    }
                }
                return that;
            }
        }

        public static readonly ISectionKey<ECDependenciesSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public List<ECDependencyInfo> ECDependencies { get; set; }
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
            writer.Write(ECDependencies.Count);
            foreach (var dependency in ECDependencies)
            {
                writer.Write(dependency.InfoVersion);
                writer.Write(dependency.FileSize);
                writer.Write(dependency.FileLastModifiedDate.ToFileTime());
                if (dependency.InfoVersion >= 2)
                {
                    writer.Write(dependency.ReExport ? 1 : 0);
                }
                else
                {
                    if (dependency.ReExport)
                    {
                        throw new Exception($"Cannot re-export EC when {nameof(dependency.InfoVersion)} is 1");
                    }
                }
                writer.WriteInt32sWithByteSizePrefix(dependency.DefinedIds.Select(x => x.Start).ToArray());
                writer.WriteInt32sWithByteSizePrefix(dependency.DefinedIds.Select(x => x.Count).ToArray());
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
