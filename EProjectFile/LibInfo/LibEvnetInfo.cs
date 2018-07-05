using Newtonsoft.Json;

namespace QIQI.EProjectFile.LibInfo
{
    public class LibEvnetInfo
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
