using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 访问对象成员表达式
    /// </summary>
    public class AccessMemberExpression : In0x38Expression
    {
        public readonly Expression Target;
        public readonly short LibraryId;
        public readonly int StructId;
        public readonly int MemberId;
        public AccessMemberExpression(Expression target, int structId, int memberId)
        {
            Target = target;
            LibraryId = -2;
            StructId = structId;
            MemberId = memberId;
        }
        public AccessMemberExpression(Expression target, int structId, short libraryId, int memberId)
        {
            Target = target;
            StructId = structId;
            LibraryId = libraryId;
            MemberId = memberId;
        }

        public override void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            Target.ToTextCode(nameMap, result, indent);
            result.Append(".");
            result.Append(LibraryId == -2 ? nameMap.GetUserDefinedName(MemberId) : nameMap.GetLibTypeMemberName(LibraryId, StructId, MemberId));
        }

        internal override void WriteTo(MethodCodeDataWriterArgs a, bool need0x1DAnd0x37)
        {
            if (need0x1DAnd0x37)
            {
                a.VariableReference.Write(a.Offest);
                a.ExpressionData.Write((byte)0x1D);
                a.ExpressionData.Write((byte)0x38);
            }
            if (Target is In0x38Expression)
            {
                ((In0x38Expression)Target).WriteTo(a, false);
            }
            else
            {
                a.ExpressionData.Write(EplSystemId.Id_NaV);
                a.ExpressionData.Write((byte)0x3A);
                Target.WriteTo(a);
            }
            a.ExpressionData.Write((byte)0x39);
            a.ExpressionData.Write(MemberId);
            if (LibraryId == -2)
                a.ExpressionData.Write(StructId);
            else
                a.ExpressionData.Write((StructId + 1) & 0xFFFF | (LibraryId + 1) << 16);
            if (need0x1DAnd0x37) a.ExpressionData.Write((byte)0x37);
        }
    }
}
