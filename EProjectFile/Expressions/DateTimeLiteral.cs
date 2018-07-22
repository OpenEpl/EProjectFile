using System;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 日期时间型字面量
    /// </summary>
    public class DateTimeLiteral : Expression
    {
        public readonly DateTime Value;

        public DateTimeLiteral(DateTime value)
        {
            this.Value = value;
        }

        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            result.Append("[");
            if (Value != null)
            {
                if (Value.TimeOfDay.TotalSeconds == 0)
                    result.Append(Value.ToString("yyyy年MM月dd日"));
                else
                    result.Append(Value.ToString("yyyy年MM月dd日HH时mm分ss秒"));
            }
            result.Append("]");

        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x19);
            a.ExpressionData.Write(Value.ToOADate());
        }
    }
}
