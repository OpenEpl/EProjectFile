using System.IO;
using System.Text;
using QIQI.EProjectFile.Expressions;
namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 如果真 语句块
    /// </summary>
    public class IfStatement : Statement
    {
        public Expression Condition { get; set; }
        /// <summary>
        /// <see cref="UnexaminedCode"/>不为null时，<see cref="Condition"/>应为null
        /// </summary>
        public string UnexaminedCode { get; set; }
        public StatementBlock Block { get; set; }
        public string Comment { get; set; }
        public bool Mask { get; set; }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (Mask)
                writer.Write("' ");
            if (UnexaminedCode == null)
            {
                writer.Write(".如果真 (");
                Condition.ToTextCode(nameMap, writer, indent);
                writer.Write(")");
            }
            else
            {
                writer.Write(".");
                writer.Write(UnexaminedCode);
            }
            if (Comment != null)
            {
                writer.Write("  ' ");
                writer.Write(Comment);
            }
            writer.WriteLine();
            Block.ToTextCode(nameMap, writer, indent + 1);
            writer.WriteLine();
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (Mask)
                writer.Write("' ");
            writer.Write(".如果真结束");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(2))
            {
                if (UnexaminedCode != null)
                    new UnexaminedStatement(UnexaminedCode, Mask).WriteTo(a, 0x6C, 0, 1);
                else
                    new ExpressionStatement(new CallExpression(0, 1, new ParamListExpression() { Condition }), Mask, Comment).WriteTo(a, 0x6C);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x52);
            }
            a.ExpressionData.Write((byte)0x73);
        }
    }
}
