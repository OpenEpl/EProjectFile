using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace QIQI.EProjectFile.LibInfo
{
    public class LibInfo
    {
        public static string LibNameInfoToJsonExecFile = null;
        static LibInfo()
        {
            LibNameInfoToJsonExecFile = Path.Combine(Path.GetDirectoryName(typeof(LibInfo).Assembly.Location), "LibNameInfoToJson.exe");
            if (!File.Exists(LibNameInfoToJsonExecFile))
            {
                LibNameInfoToJsonExecFile = Path.Combine(Path.GetDirectoryName(typeof(LibInfo).Assembly.Location), "..", "..", "build", "LibNameInfoToJson.exe");
            }
            if (!File.Exists(LibNameInfoToJsonExecFile))
            {
                LibNameInfoToJsonExecFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "LibNameInfoToJson.exe");
            }
            if (!File.Exists(LibNameInfoToJsonExecFile))
            {
                LibNameInfoToJsonExecFile = null;
            }
        }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(VersionConverter))]
        public Version VersionName { get; set; }
        public int VersionCode { get; set; }
        public LibDataTypeInfo[] DataType { get; set; }
        public LibCmdInfo[] Cmd { get; set; }
        public LibConstantInfo[] Constant { get; set; }
        public static LibInfo Load(LibraryRefInfo refInfo)
        {
            if (!File.Exists(LibNameInfoToJsonExecFile))
                throw new Exception("找不到LibNameInfoToJson.exe文件");
            string tempFile = Path.GetTempFileName();
            string result = null;
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = LibNameInfoToJsonExecFile;
                    process.StartInfo.Arguments = $"\"{refInfo.FileName}\" \"{tempFile}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                        result = File.ReadAllText(tempFile, Encoding.Unicode);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load library \"{refInfo.FileName}\"", e);
            }
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new Exception($"Failed to load library \"{refInfo.FileName}\"");
            }
            return JsonConvert.DeserializeObject<LibInfo>(result);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}