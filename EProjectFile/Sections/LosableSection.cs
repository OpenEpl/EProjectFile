﻿using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class LosableSection : ISection
    {
        private class KeyImpl : ISectionKey<LosableSection>
        {
            public string SectionName => "可丢失程序段";
            public int SectionKey => 0x05007319;
            public bool IsOptional => true;

            public LosableSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    var losableSectionInfo = new LosableSection();
                    losableSectionInfo.OutFile = reader.ReadStringWithLengthPrefix(encoding);
                    losableSectionInfo.RemovedDefinedItems = RemovedDefinedItemInfo.ReadRemovedDefinedItems(reader, encoding);
                    losableSectionInfo.UnknownAfterRemovedDefinedItem = reader.ReadImmutableBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)) switch
                    {
                        // use shared object if it equals to the dafault value, which can reduce memory usage.
                        var x when x.SequenceEqual(DefaultUnknownAfterRemovedDefinedItem) => DefaultUnknownAfterRemovedDefinedItem,
                        var x => x
                    };
                    return losableSectionInfo;
                });
            }
        }
        public static readonly ISectionKey<LosableSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public string OutFile { get; set; }
        public List<RemovedDefinedItemInfo> RemovedDefinedItems { get; set; }
        private static readonly ImmutableArray<byte> DefaultUnknownAfterRemovedDefinedItem 
            = ImmutableArray.Create(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255 } );
        [JsonIgnore]
        public ImmutableArray<byte> UnknownAfterRemovedDefinedItem { get; set; } = DefaultUnknownAfterRemovedDefinedItem;
        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer => 
            {
                var encoding = context.Encoding;
                writer.WriteStringWithLengthPrefix(encoding, OutFile);
                RemovedDefinedItemInfo.WriteRemovedDefinedItems(writer, encoding, RemovedDefinedItems);
                writer.Write(UnknownAfterRemovedDefinedItem);
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
