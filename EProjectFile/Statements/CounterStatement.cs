using System.IO;
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
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (MaskOnStart)
                writer.Write("' ");
            if (UnexaminedCode == null)
            {
                writer.Write(".计次循环首 (");
                Count.ToTextCode(nameMap, writer, indent);
                writer.Write(", ");
                Var.ToTextCode(nameMap, writer, indent);
                writer.Write(")");
            }
            else
            {
                writer.Write(".");
                writer.Write(UnexaminedCode);
            }
            if (CommentOnStart != null)
            {
                writer.Write("  ' ");
                writer.Write(CommentOnStart);
            }
            writer.WriteLine();
            Block.ToTextCode(nameMap, writer, indent + 1);
            writer.WriteLine();
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (MaskOnEnd)
                writer.Write("' ");
            writer.Write(".计次循环尾 ()");
            if (CommentOnEnd != null)
            {
                writer.Write("  ' ");
                writer.Write(CommentOnEnd);
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
