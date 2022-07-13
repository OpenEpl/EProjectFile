using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 子程序取址
    /// </summary>
    public class MethodPtrExpression : Expression
    {
        public readonly int MethodId;
        public MethodPtrExpression(int methodId)
        {
            this.MethodId = methodId;
        }

        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.MethodReference.Write(a.Offest);
            a.ExpressionData.Write((byte)0x1E);
            a.ExpressionData.Write(MethodId);
        }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            writer.Write("&");
            writer.Write(nameMap.GetUserDefinedName(MethodId));
        }
    }
}
