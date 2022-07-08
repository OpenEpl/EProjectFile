using System;

namespace QIQI.EProjectFile
{
    /// <summary>
    /// 易语言项目文件由多个数据段组成
    /// </summary>
    public struct RawSectionInfo
    {
        /// <summary>
        /// 数据段的人类辨别名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 用于决定如何对Name字段编码，同时也是易语言内部识别数据段的唯一标识（易语言内部并不靠Name字段识别数据段）
        /// </summary>
        public int Key;
        /// <summary>
        /// 数据段中的数据（不含段头）
        /// </summary>
        public byte[] Data;
        /// <summary>
        /// 如果易系统版本过低，无法识别本数据段，或本数据段已损坏，此字段将指定是否可以直接跳过。对于重要数据段，此字段通常设置为false，否则设置为true。
        /// </summary>
        public bool IsOptional;

        public RawSectionInfo(int key, string name, bool canSkip, byte[] data)
        {
            Key = key;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsOptional = canSkip;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}