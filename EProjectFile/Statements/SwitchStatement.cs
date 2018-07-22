using System;
using System.Collections.Generic;
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
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            if (Case.Count == 0)
                throw new Exception("Must hava a case");
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (Case[0].Mask)
                result.Append("' ");
            if (Case[0].UnexaminedCode == null)
            {
                result.Append(".判断开始 (");
                Case[0].Condition.ToTextCode(nameMap, result, indent);
                result.Append(")");
            }
            else
            {
                result.Append(".判断开始");
                result.Append(Case[0].UnexaminedCode.TrimStart().Substring("判断".Length));
            }
            if (Case[0].Comment != null)
            {
                result.Append("  ' ");
                result.Append(Case[0].Comment);
            }
            result.AppendLine();
            Case[0].Block.ToTextCode(nameMap, result, indent + 1);
            for (int i = 1; i < Case.Count; i++)
            {
                result.AppendLine();
                for (int x = 0; x < indent; x++)
                    result.Append("    ");
                if (Case[i].Mask)
                    result.Append("' ");
                if (Case[i].UnexaminedCode == null)
                {
                    result.Append(".判断 (");
                    Case[i].Condition.ToTextCode(nameMap, result, indent);
                    result.Append(")");
                }
                else
                {
                    result.Append(".");
                    result.Append(Case[i].UnexaminedCode);
                }
                if (Case[i].Comment != null)
                {
                    result.Append("  ' ");
                    result.Append(Case[i].Comment);
                }
                result.AppendLine();
                Case[i].Block.ToTextCode(nameMap, result, indent + 1);
            }
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            result.AppendLine(".默认");
            DefaultBlock.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            result.Append(".判断结束");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(4))
            {
                a.ExpressionData.Write((byte)0x6D);
                foreach (var curCase in Case)
                {
                    if (curCase.UnexaminedCode != null)
                        new UnexaminedStatement(curCase.UnexaminedCode, curCase.Mask).WriteTo(a, 0x6E);
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
