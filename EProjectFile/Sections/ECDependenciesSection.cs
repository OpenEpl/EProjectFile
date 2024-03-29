﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;
using QIQI.EProjectFile.Internal;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class ECDependenciesSection : ISection
    {
        private class KeyImpl : ISectionKey<ECDependenciesSection>
        {
            public string SectionName => "易模块记录段";
            public int SectionKey => 0x0C007319;
            public bool IsOptional => false;

            public ECDependenciesSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    var that = new ECDependenciesSection();
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
                    return that;
                });
            }
        }

        public static readonly ISectionKey<ECDependenciesSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public List<ECDependencyInfo> ECDependencies { get; set; }
        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer => 
            {
                var encoding = context.Encoding;
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
                    writer.WriteStringWithLengthPrefix(encoding, dependency.Name);
                    writer.WriteStringWithLengthPrefix(encoding, dependency.Path);
                    writer.WriteInt32sWithByteSizePrefix(dependency.DefinedIds.Select(x => x.Start).ToArray());
                    writer.WriteInt32sWithByteSizePrefix(dependency.DefinedIds.Select(x => x.Count).ToArray());
                }
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
