using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class LibraryRefInfo
    {
        public string FileName { get; set; }
        public string GuidString { get; set; } // 为了保证最大限度的准确还原，直接存储原始格式字符串
        public Version Version { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 用于支持库版本检查，当支持库文件所包含的 命令 数量少于此值时，IDE将发出 支持库版本不兼容 的警告
        /// </summary>
        public int MinRequiredCmd { get; set; }
        /// <summary>
        /// 用于支持库版本检查，当支持库文件所包含的 数据类型 数量少于此值时，IDE将发出 支持库版本不兼容 的警告
        /// </summary>
        public short MinRequiredDataType { get; set; }
        /// <summary>
        /// 用于支持库版本检查，当支持库文件所包含的 常量 数量少于此值时，IDE将发出 支持库版本不兼容 的警告
        /// </summary>
        public short MinRequiredConstant { get; set; }
        public static LibraryRefInfo[] ReadLibraries(BinaryReader reader, Encoding encoding)
        {
            return reader.ReadStringsWithMfcStyleCountPrefix(encoding).Select(x =>
            {
                var array = x.Split('\r');
                return new LibraryRefInfo()
                {
                    FileName = array[0],
                    GuidString = array[1],
                    Version = new Version(int.Parse(array[2]), int.Parse(array[3])),
                    Name = array[4]
                };
            }).ToArray();
        }
        public static void ApplyCompatibilityInfo(LibraryRefInfo[] infos, int[] minRequiredCmds, short[] minRequiredDataTypes, short[] minRequiredConstants)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                infos[i].MinRequiredCmd = minRequiredCmds != null && i < minRequiredCmds.Length
                    ? minRequiredCmds[i] : 0;
                infos[i].MinRequiredDataType = minRequiredDataTypes != null && i < minRequiredDataTypes.Length
                    ? minRequiredDataTypes[i] : (short)0;
                infos[i].MinRequiredConstant = minRequiredConstants != null && i < minRequiredConstants.Length
                    ? minRequiredConstants[i] : (short)0;
            }
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
