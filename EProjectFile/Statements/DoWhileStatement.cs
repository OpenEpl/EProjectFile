﻿using System.IO;
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
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (MaskOnStart)
                writer.Write("' ");
            writer.Write(".循环判断首 ()");
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
            if (UnexaminedCode == null)
            {
                writer.Write(".循环判断尾 (");
                Condition.ToTextCode(nameMap, writer, indent);
                writer.Write(")");
            }
            else
            {
                writer.Write(".");
                writer.Write(UnexaminedCode);
            }
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
                new ExpressionStatement(new CallExpression(0, 5, new ParamListExpression() { }), MaskOnStart, CommentOnStart).WriteTo(a, 0x70);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x55);
            }
            if (UnexaminedCode != null)
                new UnexaminedStatement(UnexaminedCode, MaskOnEnd).WriteTo(a, 0x71, 0, 6);
            else
                new ExpressionStatement(new CallExpression(0, 6, new ParamListExpression() { Condition }), MaskOnEnd, CommentOnEnd).WriteTo(a, 0x71);
        }
    }
}
