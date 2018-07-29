using System.Text;
using QIQI.EProjectFile.Expressions;
namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 循环判断 语句块
    /// </summary>
    public class DoWhileStatement : LoopStatement
    {
        public Expression Condition { get; set; }
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnStart)
                result.Append("' ");
            result.Append(".判断循环首 ()");
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
            if (UnexaminedCode == null)
            {
                result.Append(".判断循环尾 (");
                Condition.ToTextCode(nameMap, result, indent);
                result.Append(")");
            }
            else
            {
                result.Append(".");
                result.Append(UnexaminedCode);
            }
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
                new ExpressionStatement(new CallExpression(0, 5, new ParamListExpression() { }), MaskOnStart, CommentOnStart).WriteTo(a, 0x70);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x55);
            }
            if (UnexaminedCode != null)
                new UnexaminedStatement(UnexaminedCode, MaskOnEnd).WriteTo(a, 0x71);
            else
                new ExpressionStatement(new CallExpression(0, 6, new ParamListExpression() { Condition }), MaskOnEnd, CommentOnEnd).WriteTo(a, 0x71);
        }
    }
}
