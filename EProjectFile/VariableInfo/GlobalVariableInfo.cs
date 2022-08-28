using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace QIQI.EProjectFile
{
    public class GlobalVariableInfo : AbstractVariableInfo
    {
        public bool Public { get => (Flags & 0x100) != 0; set => Flags = (Flags & ~0x100) | (value ? 0x100 : 0); }
        public bool Hidden { get => (Flags & 0x200) != 0; set => Flags = (Flags & ~0x200) | (value ? 0x200 : 0); }
        public GlobalVariableInfo(int id) : base(EplSystemId.MakeSureIsSpecifiedType(id, EplSystemId.Type_Global))
        {
        }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            string strForUBound;
            if (UBound.Length == 0)
                strForUBound = "";
            else if (UBound.Length == 1 && UBound[0] == 0)
                strForUBound = "\"0\"";
            else
                strForUBound = "\"" + string.Join(",", UBound.Select(x => x == 0 ? "" : x.ToString())) + "\"";
            TextCodeUtils.WriteDefinitionCode(writer, indent, "全局变量", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(DataType), Public ? "公开" : "", strForUBound, Comment);
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
