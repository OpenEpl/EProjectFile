using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class DllParameterInfo : AbstractVariableInfo
    {
        public bool ArrayParameter { get => (Flags & 0x8) != 0; set => Flags = (Flags & ~0x8) | (value ? 0x8 : 0); }
        public bool ByRef { get => (Flags & 0x2) != 0; set => Flags = (Flags & ~0x2) | (value ? 0x2 : 0); }
        public DllParameterInfo(int id) : base(EplSystemId.MakeSureIsSpecifiedType(id, EplSystemId.Type_DllParameter))
        {
        }
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            string strForFlags = string.Join(" ", new string[] { ByRef ? "传址" : null, ArrayParameter ? "数组" : null }.Where(x => x != null));
            TextCodeUtils.WriteDefinedCode(result, indent, "参数", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(DataType), strForFlags, Comment);
        }
    }
}
