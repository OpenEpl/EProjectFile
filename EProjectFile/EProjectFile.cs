using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class EProjectFile
    {
        public List<ISectionInfo> Sections { get; } = new List<ISectionInfo>();
        public Encoding DetermineEncoding()
        {
            return GetOrNull(ESystemInfo.Key)?.DetermineEncoding() ?? Encoding.GetEncoding("gbk");
        }

        public TSection GetOrNull<TSection>(ISectionInfoKey<TSection> key) where TSection : ISectionInfo
        {
            if (Sections.FirstOrDefault(x => x.SectionKey == key.SectionKey) is TSection it)
            {
                return it;
            }
            else
            {
                return default;
            }
        }

        public TSection Get<TSection>(ISectionInfoKey<TSection> key) where TSection : ISectionInfo
        {
            return (TSection)Sections.First(x => x.SectionKey == key.SectionKey);
        }

        public void Load(Stream stream, ProjectFileReader.OnInputPassword inputPassword = null)
        {
            var encoding = Encoding.GetEncoding("gbk");
            Sections.Clear();
            using (var reader = new ProjectFileReader(stream, inputPassword))
            {
                while (!reader.IsFinish)
                {
                    var rawSection = reader.ReadSection();
                    ISectionInfo section;
                    if (PredefinedSections.Keys.TryGetValue(rawSection.Key, out var sectionKey))
                    {
                        section = sectionKey.Parse(rawSection.Data, encoding, reader.CryptEc);
                    }
                    else
                    {
                        section = new GeneralSection(rawSection);
                    }
                    if (section is ESystemInfo systemInfo)
                    {
                        encoding = systemInfo.DetermineEncoding();
                    }
                    if (!(section is EndOfFileSection))
                    {
                        Sections.Add(section);
                    }
                }
            }
        }
        public void Save(Stream stream)
        {
            var encoding = Encoding.GetEncoding("gbk");
            using (var writer = new ProjectFileWriter(stream))
            {
                foreach (var section in Sections)
                {
                    if (section is ESystemInfo systemInfo)
                    {
                        encoding = systemInfo.DetermineEncoding();
                    }
                    writer.WriteSection(new RawSectionInfo(section.SectionKey, section.SectionName, section.IsOptional, section.ToBytes(encoding)));
                }
                {
                    var section = EndOfFileSection.Instance;
                    writer.WriteSection(new RawSectionInfo(section.SectionKey, section.SectionName, section.IsOptional, section.ToBytes(encoding)));
                }
            }
        }
    }
}
