using System;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 逻辑型字面量
    /// </summary>
    public class BoolLiteral : Expression
    {
#pragma warning disable CS0612 // 类型或成员已过时
        public static readonly BoolLiteral True = new BoolLiteral(true);
        public static readonly BoolLiteral False = new BoolLiteral(false);
#pragma warning restore CS0612 // 类型或成员已过时
        public static BoolLiteral ValueOf(bool x) => x ? True : False;
        public readonly bool Value;
        [Obsolete]
        public BoolLiteral(bool value)
        {
            this.Value = value;
        }
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            result.Append(Value ? "真" : "假");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x18);
            a.ExpressionData.Write((short)(Value ? -1 : 0));
        }
    }
}
