using QIQI.EProjectFile.Context;
using QIQI.EProjectFile.Encryption;
using QIQI.EProjectFile.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class EplDocument
    {
        public List<ISection> Sections { get; } = new List<ISection>();
        public Encoding DetermineEncoding()
        {
            return GetOrNull(ESystemInfoSection.Key)?.DetermineEncoding() ?? Encoding.GetEncoding("gbk");
        }

        public TSection GetOrNull<TSection>(ISectionKey<TSection> key) where TSection : ISection
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

        public TSection Get<TSection>(ISectionKey<TSection> key) where TSection : ISection
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
                    ISection section;
                    if (PredefinedSections.Keys.TryGetValue(rawSection.Key, out var sectionKey))
                    {
                        section = sectionKey.Parse(new BlockParserContext(rawSection.Data, encoding, reader.CryptEC));
                    }
                    else
                    {
                        section = new GeneralSection(rawSection);
                    }
                    if (section is ESystemInfoSection systemInfo)
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
        public void Save(Stream stream) => Save(stream, null);
        public void Save(Stream stream, EplEncryptionOptions encryptionOptions)
        {
            var encoding = Encoding.GetEncoding("gbk");
            using (var writer = new ProjectFileWriter(stream, encryptionOptions))
            {
                var context = new BlockByteifierContext(encoding, writer.CryptEC);
                foreach (var section in Sections)
                {
                    if (section is ESystemInfoSection systemInfo)
                    {
                        encoding = systemInfo.DetermineEncoding();
                    }
                    writer.WriteSection(new RawSectionInfo(section.SectionKey, section.SectionName, section.IsOptional, section.ToBytes(context)));
                }
                {
                    var section = EndOfFileSection.Instance;
                    writer.WriteSection(new RawSectionInfo(section.SectionKey, section.SectionName, section.IsOptional, section.ToBytes(context)));
                }
            }
        }
    }
}
