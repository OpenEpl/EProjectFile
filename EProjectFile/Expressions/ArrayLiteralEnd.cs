using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 解析时临时标记（数组字面量结束标识）
    /// </summary>
    internal class ArrayLiteralEnd : Expression
    {
        public static readonly ArrayLiteralEnd Instance = new ArrayLiteralEnd();
        private ArrayLiteralEnd()
        {
            if (Instance != null) throw new NotSupportedException();
        }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0) => throw new NotImplementedException();

        internal override void WriteTo(MethodCodeDataWriterArgs a) => a.ExpressionData.Write((byte)0x20);
    }
}
