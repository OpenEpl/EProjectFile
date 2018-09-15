using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class EProjectFile
    {
        public Encoding Encoding => ESystemInfo != null ? ESystemInfo.Encoding : Encoding.GetEncoding("gbk");
        public ESystemInfo ESystemInfo { get; set; }
        public ProjectConfigInfo ProjectConfigInfo { get; set; }
        public ResourceSectionInfo Resource { get; set; }
        public CodeSectionInfo Code { get; set; }
        public EPackageInfo EPackageInfo { get; set; }
        public InitEcSectionInfo InitEcSectionInfo { get; set; }
        public byte[] EcSection { get; set; }
        public byte[] EcSection2 { get; set; }
        public byte[] AuxiliarySection2 { get; set; }
        public LosableSectionInfo LosableSection { get; set; }
        public byte[] AuxiliarySection3 { get; set; }
        public byte[] EditInfoSection2 { get; set; }
        public byte[] AuxiliarySection1 { get; set; }
        public FolderSectionInfo FolderSection { get; set; }
        public List<SectionInfo> OtherSections { get; set; }
        public void Load(Stream stream, ProjectFileReader.OnInputPassword inputPassword = null)
        {
            ESystemInfo = null;
            ProjectConfigInfo = null;
            Resource = null;
            Code = null;
            EPackageInfo = null;
            InitEcSectionInfo = null;
            LosableSection = null;
            FolderSection = null;
            EcSection = EcSection2 = AuxiliarySection2 = AuxiliarySection3 = EditInfoSection2 = AuxiliarySection1 = null;
            OtherSections = new List<SectionInfo>();
            using (var reader = new ProjectFileReader(stream, inputPassword))
            {
                var processor = new Dictionary<int, Action<SectionInfo>>
                {
                    { ESystemInfo.SectionKey, x => ESystemInfo = ESystemInfo.Parse(x.Data) },
                    { ProjectConfigInfo.SectionKey, x => ProjectConfigInfo = ProjectConfigInfo.Parse(x.Data, Encoding) },
                    { ResourceSectionInfo.SectionKey, x => Resource = ResourceSectionInfo.Parse(x.Data, Encoding) },
                    { CodeSectionInfo.SectionKey, x => Code = CodeSectionInfo.Parse(x.Data, Encoding, reader.CryptEc) },
                    { EPackageInfo.SectionKey, x => EPackageInfo = EPackageInfo.Parse(x.Data, Encoding) },
                    { InitEcSectionInfo.SectionKey, x => InitEcSectionInfo = InitEcSectionInfo.Parse(x.Data, Encoding) },
                    { 0x0C007319, x => EcSection = x.Data },
                    { 0x0F007319, x => EcSection2 = x.Data },
                    { 0x0B007319, x => AuxiliarySection2 = x.Data },
                    { LosableSectionInfo.SectionKey, x => LosableSection = LosableSectionInfo.Parse(x.Data, Encoding) },
                    { 0x10007319, x => AuxiliarySection3 = x.Data },
                    { 0x09007319, x => EditInfoSection2 = x.Data },
                    { 0x0A007319, x => AuxiliarySection1 = x.Data },
                    { FolderSectionInfo.SectionKey, x => FolderSection = FolderSectionInfo.Parse(x.Data, Encoding) }
                };
                
                while (!reader.IsFinish)
                {
                    var section = reader.ReadSection();
                    switch (section.Key)
                    {
                        case int key when processor.ContainsKey(key):
                            processor[key](section);
                            break;
                        default:
                            OtherSections.Add(section);
                            break;
                    }
                }
            }
        }
        public void Save(Stream stream)
        {
            using (var writer = new ProjectFileWriter(stream))
            {
                if (ESystemInfo != null) writer.WriteSection(new SectionInfo(ESystemInfo.SectionKey, ESystemInfo.SectionName, false, ESystemInfo.ToBytes()));
                if (ProjectConfigInfo != null) writer.WriteSection(new SectionInfo(ProjectConfigInfo.SectionKey, ProjectConfigInfo.SectionName, true, ProjectConfigInfo.ToBytes(Encoding)));
                if (Resource != null) writer.WriteSection(new SectionInfo(ResourceSectionInfo.SectionKey, ResourceSectionInfo.SectionName, false, Resource.ToBytes(Encoding)));
                if (Code != null) writer.WriteSection(new SectionInfo(CodeSectionInfo.SectionKey, CodeSectionInfo.SectionName, false, Code.ToBytes(Encoding)));
                if (EPackageInfo != null) writer.WriteSection(new SectionInfo(EPackageInfo.SectionKey, EPackageInfo.SectionName, true, EPackageInfo.ToBytes(Encoding)));
                if (InitEcSectionInfo != null) writer.WriteSection(new SectionInfo(InitEcSectionInfo.SectionKey, InitEcSectionInfo.SectionName, false, InitEcSectionInfo.ToBytes(Encoding)));
                if (EcSection != null) writer.WriteSection(new SectionInfo(0x0C007319, "易模块记录段", false, EcSection));
                if (EcSection2 != null) writer.WriteSection(new SectionInfo(0x0F007319, "易模块记录段2", true, EcSection2));
                if (AuxiliarySection2 != null) writer.WriteSection(new SectionInfo(0x0B007319, "辅助信息段2", true, AuxiliarySection2));
                if (LosableSection != null) writer.WriteSection(new SectionInfo(LosableSectionInfo.SectionKey, LosableSectionInfo.SectionName, true, LosableSection.ToBytes(Encoding)));
                if (AuxiliarySection3 != null) writer.WriteSection(new SectionInfo(0x10007319, "辅助信息段3", true, AuxiliarySection3));
                if (EditInfoSection2 != null) writer.WriteSection(new SectionInfo(0x09007319, "编辑信息段2", true, EditInfoSection2));
                if (AuxiliarySection1 != null) writer.WriteSection(new SectionInfo(0x0A007319, "辅助信息段1", true, AuxiliarySection1));
                if (FolderSection != null) writer.WriteSection(new SectionInfo(FolderSectionInfo.SectionKey, FolderSectionInfo.SectionName, true, FolderSection.ToBytes(Encoding)));
                OtherSections?.ForEach(x => writer.WriteSection(x));
            }
        }
    }
}
