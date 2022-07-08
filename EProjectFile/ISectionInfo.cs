using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public interface ISectionInfo
    {
        string SectionName { get; }
        int SectionKey { get; }
        bool IsOptional { get; }
        byte[] ToBytes(Encoding encoding);
    }

    /// <summary>
    /// 注意：不是所有的 ISectionInfo 都必须带有 Key
    /// </summary>
    /// <typeparam name="TSection"></typeparam>
    public interface ISectionInfoKey<out TSection> where TSection : ISectionInfo
    {
        string SectionName { get; }
        int SectionKey { get; }
        bool IsOptional { get; }
        TSection Parse(byte[] data, Encoding encoding, bool cryptEC);
    }
}
