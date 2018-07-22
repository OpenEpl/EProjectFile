using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace QIQI.EProjectFile
{
    public class EPackageInfo
    {
        public const string SectionName = "易包信息段1";
        /// <summary>
        /// 与每个子程序一一对应，null表示对应子程序非调用易包的子程序
        /// </summary>
        public string[] FileNames { get; set; }
        public static EPackageInfo Parse(byte[] data)
        {
            var packageInfo = new EPackageInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false))) {
                var nameList = new List<string>();
                while (!(reader.BaseStream.Position == reader.BaseStream.Length))
                {
                    var name = reader.ReadStringWithLengthPrefix();
                    if ("".Equals(name)) 
                    {
                        name = null;
                    }
                    nameList.Add(name);
                }
                packageInfo.FileNames = nameList.ToArray();
            }
            return packageInfo;
        }
        public byte[] ToBytes()
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                WriteTo(writer);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }
        private void WriteTo(BinaryWriter writer)
        {
            Array.ForEach(FileNames, x => writer.WriteStringWithLengthPrefix(x));
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
