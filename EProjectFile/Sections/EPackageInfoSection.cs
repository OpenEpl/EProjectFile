using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Sections
{
    public class EPackageInfoSection : ISection
    {
        private class KeyImpl : ISectionKey<EPackageInfoSection>
        {
            public string SectionName => "易包信息段1";
            public int SectionKey => 0x0D007319;
            public bool IsOptional => true;

            public EPackageInfoSection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                var packageInfo = new EPackageInfoSection();
                using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
                {
                    var nameList = new List<string>();
                    while (!(reader.BaseStream.Position == reader.BaseStream.Length))
                    {
                        var name = reader.ReadStringWithLengthPrefix(encoding);
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
        }

        public static readonly ISectionKey<EPackageInfoSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        /// <summary>
        /// 与每个子程序一一对应，null表示对应子程序非调用易包的子程序
        /// </summary>
        public string[] FileNames { get; set; }
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
            Array.ForEach(FileNames, x => writer.WriteStringWithLengthPrefix(encoding, x));
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
