using Newtonsoft.Json;

namespace QIQI.EProjectFile.LibInfo
{
    public class LibDataTypeInfo
    {
        public string Name { get; set; }
        public string EnglshName { get; set; }
        public LibEvnetInfo[] Evnet { get; set; }
        public LibPropertyInfo[] Property { get; set; }
        public LibMemberInfo[] Member { get; set; }
        public int[] Method { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
