using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 访问常量
    /// </summary>
    public class ConstantExpression : Expression
    {
        public readonly short LibraryId;
        public readonly int ConstantId;
        public ConstantExpression(short libraryId, int constantId)
        {
            this.LibraryId = libraryId;
            this.ConstantId = constantId;
        }
        public ConstantExpression(int constantId) : this(-2, constantId)
        {
        }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            writer.Write("#");
            writer.Write(LibraryId == -2 ? nameMap.GetUserDefinedName(ConstantId) : nameMap.GetLibConstantName(LibraryId, ConstantId));
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            if (LibraryId == -2)
            {
                a.ConstantReference.Write(a.Offest);
                a.ExpressionData.Write((byte)0x1B);
                a.ExpressionData.Write(ConstantId);
            }
            else
            {
                a.ExpressionData.Write((byte)0x1C);
                a.ExpressionData.Write((short)(LibraryId + 1));
                a.ExpressionData.Write((short)(ConstantId + 1));
            }
        }
    }
}
