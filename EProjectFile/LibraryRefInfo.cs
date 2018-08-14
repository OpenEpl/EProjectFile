using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }
        public string Name { get; set; }
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
        public static void WriteLibraries(BinaryWriter writer, Encoding encoding, LibraryRefInfo[] methods)
        {
            writer.WriteStringsWithMfcStyleCountPrefix(encoding, methods.Select(x => $"{x.FileName}\r{x.GuidString}\r{x.Version.Major}\r{x.Version.Minor}\r{x.Name}").ToArray());
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
