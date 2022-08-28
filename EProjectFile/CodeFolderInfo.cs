using QIQI.EProjectFile.Internal;
using System.IO;
using System.Text;
using System.Text.Json;

namespace QIQI.EProjectFile
{
    public class CodeFolderInfo
    {
        public bool Expand { get; set; }
        public int Key { get; }
        public int ParentKey { get; set; }
        public string Name { get; set; }
        public int[] Children { get; set; }
        public CodeFolderInfo(int key)
        {
            this.Key = key;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}