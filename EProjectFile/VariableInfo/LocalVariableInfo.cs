using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace QIQI.EProjectFile
{
    public class LocalVariableInfo : AbstractVariableInfo
    {
        public bool Static { get => (Flags & 0x1) != 0; set => Flags = (Flags & ~0x1) | (value ? 0x1 : 0); }
        public LocalVariableInfo(int id) : base(EplSystemId.MakeSureIsSpecifiedType(id, EplSystemId.Type_Local))
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
            TextCodeUtils.WriteDefinitionCode(writer, indent, "局部变量", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(DataType), Static ? "静态" : "", strForUBound, Comment);
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
