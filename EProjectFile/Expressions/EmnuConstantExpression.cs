using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 访问枚举（支持库）
    /// </summary>
    public class EmnuConstantExpression : Expression
    {
        public readonly short LibraryId;
        public readonly short StructId;
        public readonly int MemberId;
        public EmnuConstantExpression(short structId, short libraryId, int memberId)
        {
            this.StructId = structId;
            this.LibraryId = libraryId;
            this.MemberId = memberId;
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x23);
            a.ExpressionData.Write((short)(StructId + 1));
            a.ExpressionData.Write((short)(LibraryId + 1));
            a.ExpressionData.Write(MemberId + 1);
        }
        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            result.Append("#");
            result.Append(nameMap.GetLibTypeName(LibraryId, StructId));
            result.Append(".");
            result.Append(nameMap.GetLibTypeMemberName(LibraryId, StructId, MemberId));
        }
    }
}
