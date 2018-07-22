using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 访问变量表达式
    /// </summary>
    public class VariableExpression : In0x38Expression
    {
        public readonly int Id;
        public VariableExpression(int id)
        {
            this.Id = id;
        }

        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            result.Append(nameMap.GetUserDefinedName(Id));
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a, bool need0x1DAnd0x37)
        {
            if (need0x1DAnd0x37)
            {
                a.VariableReference.Write(a.Offest);
                a.ExpressionData.Write((byte)0x1D);
                a.ExpressionData.Write((byte)0x38);
            }
            a.ExpressionData.Write(Id);
            if (need0x1DAnd0x37) a.ExpressionData.Write((byte)0x37);
        }
    }
}
