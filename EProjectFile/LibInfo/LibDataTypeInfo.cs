using Newtonsoft.Json;
using System.ComponentModel;

namespace QIQI.EProjectFile.LibInfo
{
    public class LibDataTypeInfo
    {
        public string Name { get; set; }
        public string EnglshName { get; set; }
        public LibEvnetInfo[] Evnet { get; set; }
        public LibMemberInfo[] Member { get; set; }
        public int[] Method { get; set; }
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsEnum { get; set; } = false;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
