using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public class MethodParameterInfo : AbstractVariableInfo
    {
        public bool OptionalParameter { get => (Flags & 0x4) != 0; set => Flags = (Flags & ~0x4) | (value ? 0x4 : 0); }
        public bool ArrayParameter { get => (Flags & 0x8) != 0; set => Flags = (Flags & ~0x8) | (value ? 0x8 : 0); }
        public bool ByRef { get => (Flags & 0x2) != 0; set => Flags = (Flags & ~0x2) | (value ? 0x2 : 0); }
        public MethodParameterInfo(int id) : base(EplSystemId.MakeSureIsSpecifiedType(id, EplSystemId.Type_Local))
        {
        }

        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            string strForFlags = string.Join(" ", new string[] { ByRef ? "参考" : null, OptionalParameter ? "可空" : null, ArrayParameter ? "数组" : null }.Where(x => x != null));
            TextCodeUtils.WriteDefinedCode(result, indent, "参数", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(DataType), strForFlags, Comment);
        }
    }
}
