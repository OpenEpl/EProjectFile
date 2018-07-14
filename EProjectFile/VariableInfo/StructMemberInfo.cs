using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class StructMemberInfo : AbstractVariableInfo
    {
        public bool ByRef { get => (Flags & 0x2) != 0; set => Flags = (Flags & ~0x2) | (value ? 0x2 : 0); }
        public StructMemberInfo(int id) : base(EplSystemId.MakeSureIsSpecifiedType(id, EplSystemId.Type_StructMember))
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
            TextCodeUtils.WriteDefinedCode(result, indent, "成员", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(DataType), ByRef ? "传址" : "", strForUBound, Comment);
        }
    }
}
