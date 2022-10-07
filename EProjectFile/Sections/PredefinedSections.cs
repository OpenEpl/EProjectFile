using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile.Sections
{
    public class PredefinedSections
    {
        public static Dictionary<int, ISectionKey<ISection>> Keys { get; } = new ISectionKey<ISection>[]{
            ESystemInfoSection.Key,
            ProjectConfigSection.Key,
            ResourceSection.Key,
            CodeSection.Key,
            EPackageInfoSection.Key,
            InitECSection.Key,
            LosableSection.Key,
            ClassPublicitySection.Key,
            FolderSection.Key,
            ECDependenciesSection.Key,
            ProjectConfigExSection.Key,
            ConditionalCompilationSection.Key,
            EditorInfoSection.Key,
            EndOfFileSection.Key
        }.ToDictionary(x => x.SectionKey);
    }
}
