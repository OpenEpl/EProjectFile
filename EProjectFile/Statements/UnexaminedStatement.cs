using System;
using System.Text;
using QIQI.EProjectFile.Expressions;
namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 未验证代码语句
    /// </summary>
    public class UnexaminedStatement : Statement
    {
        private string unexaminedCode;
        public bool Mask { get; set; }

        public string UnexaminedCode { get => unexaminedCode; set => unexaminedCode = value ?? throw new ArgumentNullException(nameof(UnexaminedCode)); }

        public UnexaminedStatement()
        {
        }

        public UnexaminedStatement(string unexaminedCode, bool mask)
        {
            UnexaminedCode = unexaminedCode;
            Mask = mask;
        }
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (Mask)
                result.Append("' ");
            result.Append(unexaminedCode);
        }
        internal void WriteTo(MethodCodeDataWriterArgs a, byte type)
        {
            a.LineOffest.Write(a.Offest);
            a.ExpressionData.Write(type);
            a.ExpressionData.Write(0);
            a.ExpressionData.Write((short)-1);
            a.ExpressionData.Write(Mask);
            a.ExpressionData.WriteBStr(a.Encoding, UnexaminedCode);
            a.ExpressionData.WriteBStr(a.Encoding, null);
            a.ExpressionData.Write((byte)0x36);
            ParamListEnd.Instance.WriteTo(a);

        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            WriteTo(a, 0x6A);

        }
    }
}
