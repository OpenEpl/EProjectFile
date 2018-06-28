using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Linq;

namespace QIQI.EProjectFile
{
    public class LibraryInfo
    {
        public string FileName;
        public string GuidString;//为了保证最大限度的准确还原，直接存储原始格式字符串
        [JsonConverter(typeof(VersionConverter))]
        public Version Version;
        public string Name;
        public static LibraryInfo[] ReadLibraries(BinaryReader reader)
        {
            return reader.ReadStringsWithMfcStyleCountPrefix().Select(x =>
            {
                var array = x.Split('\r');
                return new LibraryInfo()
                {
                    FileName = array[0],
                    GuidString = array[1],
                    Version = new Version(int.Parse(array[2]), int.Parse(array[3])),
                    Name = array[4]
                };
            }).ToArray();
        }
        public static void WriteLibraries(BinaryWriter writer, LibraryInfo[] methods)
        {
            writer.WriteStringsWithMfcStyleCountPrefix(methods.Select(x => $"{x.FileName}\r{x.GuidString}\r{x.Version.Major}\r{x.Version.Minor}\r{x.Name}").ToArray());
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
