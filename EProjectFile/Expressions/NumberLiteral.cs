using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 数值型字面量（易语言内部统一按double处理）
    /// </summary>
    public class NumberLiteral : Expression
    {
        public readonly double Value;
        public NumberLiteral(double value)
        {
            this.Value = value;
        }
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            result.Append(Value);
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x17);
            a.ExpressionData.Write(Value);
        }
    }
}
