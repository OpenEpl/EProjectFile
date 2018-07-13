using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class GlobalVariableInfo : AbstractVariableInfo
    {
        public bool Public { get => (Flags & 0x100) != 0; set => Flags = (Flags & ~0x100) | (value ? 0x100 : 0); }
        public GlobalVariableInfo(int id) : base(EplSystemId.MakeSureIsSpecifiedType(id, EplSystemId.Type_Global))
        {
        }
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            string strForUBound;
            if (UBound.Length == 0)
                strForUBound = "";
            else if (UBound.Length == 1 && UBound[0] == 0)
                strForUBound = "\"0\"";
            else
                strForUBound = "\"" + string.Join(",", UBound.Select(x => x == 0 ? "" : x.ToString())) + "\"";
            TextCodeUtils.WriteDefinedCode(result, indent, "全局变量", Name, nameMap.GetDataTypeName(DataType), Public ? "公开" : "", strForUBound, Comment);
        }
    }
}
