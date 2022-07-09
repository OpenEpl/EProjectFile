using System;
using System.Collections.Generic;
using System.Text;

namespace QIQI.EProjectFile
{
    public class ECDependencyInfo
    {
        public struct PackedIds
        {
            public int Start;
            public int Count;
        }

        public int InfoVersion { get; set; }
        public int FileSize { get; set; }
        public DateTime FileLastModifiedDate { get; set; }
        public bool ReExport { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public List<PackedIds> DefinedIds { get; set; }
    }
}
