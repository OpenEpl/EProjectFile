using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 访问数组成员表达式，多维数组通过多个AccessArrayExpression嵌套表示
    /// </summary>
    public class AccessArrayExpression : In0x38Expression
    {
        public readonly Expression Target;
        public readonly Expression Index;
        public AccessArrayExpression(Expression target, Expression index)
        {
            this.Target = target;
            this.Index = index;
        }

        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            Target.ToTextCode(nameMap, writer, indent);
            writer.Write("[");
            Index.ToTextCode(nameMap, writer, indent);
            writer.Write("]");
        }

        internal override void WriteTo(MethodCodeDataWriterArgs a, bool need0x1DAnd0x37)
        {
            if (need0x1DAnd0x37)
            {
                a.VariableReference.Write(a.Offest);
                a.ExpressionData.Write((byte)0x1D);
                a.ExpressionData.Write((byte)0x38);
            }
            if (Target is In0x38Expression)
            {
                ((In0x38Expression)Target).WriteTo(a, false);
            }
            else
            {
                a.ExpressionData.Write(EplSystemId.Id_NaV);
                a.ExpressionData.Write((byte)0x3A);
                Target.WriteTo(a);
            }
            a.ExpressionData.Write((byte)0x3A);
            if (Index is NumberLiteral)
            {
                a.ExpressionData.Write((byte)0x3B);
                a.ExpressionData.Write((int)((NumberLiteral)Index).Value);
            }
            else if (Index is In0x38Expression)
            {
                a.ExpressionData.Write((byte)0x38);
                ((In0x38Expression)Index).WriteTo(a, false);
                a.ExpressionData.Write((byte)0x37);
            }
            else
            {
                Index.WriteTo(a);
            }
            if (need0x1DAnd0x37) a.ExpressionData.Write((byte)0x37);
        }
    }
}
