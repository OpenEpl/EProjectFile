using System;
using System.IO;
using System.Text;
using QIQI.EProjectFile.Expressions;
using QIQI.EProjectFile.Internal;

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
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            if (Mask)
                writer.Write("' ");
            writer.Write(unexaminedCode);
        }
        internal void WriteTo(MethodCodeDataWriterArgs a, byte type, short libraryId, int methodId)
        {
            a.LineOffest.Write(a.Offest);
            a.ExpressionData.Write(type);
            a.ExpressionData.Write(methodId);
            a.ExpressionData.Write(libraryId);
            a.ExpressionData.Write((short)(Mask ? 0 : 0x40));
            a.ExpressionData.WriteBStr(a.Encoding, UnexaminedCode);
            a.ExpressionData.WriteBStr(a.Encoding, null);
            a.ExpressionData.Write((byte)0x36);
            ParamListEnd.Instance.WriteTo(a);

        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            WriteTo(a, 0x6A, -1, 0);

        }
    }
}
