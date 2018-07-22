using System.Text;
using QIQI.EProjectFile.Expressions;
namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 计次循环 语句块
    /// </summary>
    public class CounterStatement : LoopStatement
    {
        public Expression Count { get; set; }
        public Expression Var { get; set; }
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnStart)
                result.Append("' ");
            if (UnexaminedCode == null)
            {
                result.Append(".计次循环首 (");
                Count.ToTextCode(nameMap, result, indent);
                result.Append(", ");
                Var.ToTextCode(nameMap, result, indent);
                result.Append(")");
            }
            else
            {
                result.Append(".");
                result.Append(UnexaminedCode);
            }
            if (CommentOnStart != null)
            {
                result.Append("  ' ");
                result.Append(CommentOnStart);
            }
            result.AppendLine();
            Block.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnEnd)
                result.Append("' ");
            result.Append(".计次循环尾 ()");
            if (CommentOnEnd != null)
            {
                result.Append("  ' ");
                result.Append(CommentOnEnd);
            }
        }

        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(3))
            {
                if (UnexaminedCode != null)
                    new UnexaminedStatement(UnexaminedCode, MaskOnStart).WriteTo(a, 0x70);
                else
                    new ExpressionStatement(new CallExpression(0, 7, new ParamListExpression() { Count, Var }), MaskOnStart, CommentOnStart).WriteTo(a, 0x70);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x55);
            }
            new ExpressionStatement(new CallExpression(0, 8, new ParamListExpression() { }), MaskOnEnd, CommentOnEnd).WriteTo(a, 0x71);
        }
    }
}
