using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    public class DefaultValueExpression : Expression
    {
        public static readonly DefaultValueExpression Instance = new DefaultValueExpression();
        private DefaultValueExpression()
        {
            if (Instance != null) throw new NotSupportedException();
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a) => a.ExpressionData.Write((byte)0x16);
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            // Nothing need doing.
        }
    }
}
