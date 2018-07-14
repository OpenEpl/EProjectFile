using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class ClassVariableInfo : AbstractVariableInfo
    {
        public ClassVariableInfo(int id) : base(EplSystemId.MakeSureIsSpecifiedType(id, EplSystemId.Type_ClassMember))
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
            TextCodeUtils.WriteDefinedCode(result, indent, "程序集变量", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(DataType),  "", strForUBound, Comment);
        }
    }
}
