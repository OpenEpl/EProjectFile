using QIQI.EProjectFile.Internal;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 文本型字面量
    /// </summary>
    public class StringLiteral : Expression
    {
        public readonly string Value;
        public StringLiteral(string value)
        {
            this.Value = value;
        }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            writer.Write("“");
            writer.Write(Value ?? "");
            writer.Write("”");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x1A);
            a.ExpressionData.WriteBStr(a.Encoding, Value ?? "");
        }
    }
}
