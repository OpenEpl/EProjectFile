using QIQI.EProjectFile.Context;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile.Sections
{
    [JsonConverter(typeof(SectionJsonConverter))]
    public interface ISection
    {
        string SectionName { get; }
        int SectionKey { get; }
        bool IsOptional { get; }
        byte[] ToBytes(BlockByteifierContext context);
    }

    /// <summary>
    /// 注意：不是所有的 ISection 都必须带有 Key
    /// </summary>
    /// <typeparam name="TSection"></typeparam>
    public interface ISectionKey<out TSection> where TSection : ISection
    {
        string SectionName { get; }
        int SectionKey { get; }
        bool IsOptional { get; }
        TSection Parse(BlockParserContext context);
    }
}
