using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QIQI.EProjectFile.Expressions;
namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 判断 语句块
    /// </summary>
    public class SwitchStatement : Statement
    {
        public class CaseInfo
        {
            public Expression Condition { get; set; }
            public string UnexaminedCode { get; set; }
            public StatementBlock Block { get; set; }
            public string Comment { get; set; }
            public bool Mask { get; set; }
        }
        public List<CaseInfo> Case { get; } = new List<CaseInfo>();
        public StatementBlock DefaultBlock { get; set; }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            if (Case.Count == 0)
                throw new Exception("Must hava a case");
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (Case[0].Mask)
                writer.Write("' ");
            if (Case[0].UnexaminedCode == null)
            {
                writer.Write(".判断开始 (");
                Case[0].Condition.ToTextCode(nameMap, writer, indent);
                writer.Write(")");
            }
            else
            {
                writer.Write(".判断开始");
                writer.Write(Case[0].UnexaminedCode.TrimStart().Substring("判断".Length));
            }
            if (Case[0].Comment != null)
            {
                writer.Write("  ' ");
                writer.Write(Case[0].Comment);
            }
            writer.WriteLine();
            Case[0].Block.ToTextCode(nameMap, writer, indent + 1);
            for (int i = 1; i < Case.Count; i++)
            {
                writer.WriteLine();
                for (int x = 0; x < indent; x++)
                    writer.Write("    ");
                if (Case[i].Mask)
                    writer.Write("' ");
                if (Case[i].UnexaminedCode == null)
                {
                    writer.Write(".判断 (");
                    Case[i].Condition.ToTextCode(nameMap, writer, indent);
                    writer.Write(")");
                }
                else
                {
                    writer.Write(".");
                    writer.Write(Case[i].UnexaminedCode);
                }
                if (Case[i].Comment != null)
                {
                    writer.Write("  ' ");
                    writer.Write(Case[i].Comment);
                }
                writer.WriteLine();
                Case[i].Block.ToTextCode(nameMap, writer, indent + 1);
            }
            writer.WriteLine();
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (Case[0].Mask)
                writer.Write("' ");
            writer.WriteLine(".默认");
            DefaultBlock.ToTextCode(nameMap, writer, indent + 1);
            writer.WriteLine();
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (Case[0].Mask)
                writer.Write("' ");
            writer.Write(".判断结束");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(4))
            {
                a.ExpressionData.Write((byte)0x6D);
                foreach (var curCase in Case)
                {
                    if (curCase.UnexaminedCode != null)
                        new UnexaminedStatement(curCase.UnexaminedCode, curCase.Mask).WriteTo(a, 0x6E, 0, 2);
                    else
                        new ExpressionStatement(new CallExpression(0, 2, new ParamListExpression() { curCase.Condition }), curCase.Mask, curCase.Comment).WriteTo(a, 0x6E);
                    curCase.Block.WriteTo(a);
                    a.ExpressionData.Write((byte)0x53);
                }
                a.ExpressionData.Write((byte)0x6F);
                DefaultBlock.WriteTo(a);
                a.ExpressionData.Write((byte)0x54);
            }
            a.ExpressionData.Write((byte)0x74);
        }
    }
}
