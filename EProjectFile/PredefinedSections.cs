using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class PredefinedSections
    {
        public static Dictionary<int, ISectionInfoKey<ISectionInfo>> Keys = new ISectionInfoKey<ISectionInfo>[]{
            ESystemInfo.Key,
            ProjectConfigInfo.Key,
            ResourceSectionInfo.Key,
            CodeSectionInfo.Key,
            EPackageInfo.Key,
            InitEcSectionInfo.Key,
            LosableSectionInfo.Key,
            FolderSectionInfo.Key,
            ECDependenciesSectionInfo.Key,
            EndOfFileSection.Key
        }.ToDictionary(x => x.SectionKey);
    }
}
