using System.Text;
using QIQI.EProjectFile.Expressions;
namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 表达式语句
    /// </summary>
    public class ExpressionStatement : Statement
    {
        public CallExpression Expression { get; set; }
        public bool Mask { get; set; }

        public string Comment { get; set; }
        public ExpressionStatement()
        {

        }

        public ExpressionStatement(CallExpression expression, bool mask, string comment)
        {
            Expression = expression;
            Mask = mask;
            Comment = comment;
        }

        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (Mask)
                result.Append("' ");
            Expression?.ToTextCode(nameMap, result, indent);
            if (Comment != null)
            {
                result.Append(Expression == null ? "' " : "  ' ");
                result.Append(Comment);
            }
        }

        internal void WriteTo(MethodCodeDataWriterArgs a, byte type)
        {
            a.LineOffest.Write(a.Offest);
            if (Expression != null)
            {
                Expression.WriteTo(a, type, Mask, Comment);
            }
            else
            {
                new CallExpression(-1, 0).WriteTo(a, type, Mask, Comment);
            }
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            WriteTo(a, 0x6A);
        }
    }
}
