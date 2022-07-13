using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 常规子程序调用表达式
    /// </summary>
    public class CallExpression : Expression
    {
        /// <summary>
        /// >=0：支持库索引，-1：空，-2：用户定义子程序，-3：外部DLL命令
        /// </summary>
        public readonly short LibraryId;
        public readonly int MethodId;
        /// <summary>
        /// 忽略虚函数表，调用父类特定方法时使用
        /// </summary>
        public bool InvokeSpecial { get; set; } = false;
        public Expression Target { get; set; } // ThisCall
        public ParamListExpression ParamList { get; set; } = null;
        public CallExpression(short libraryId, int methodId, ParamListExpression paramList = null)
        {
            LibraryId = libraryId;
            MethodId = methodId;
            ParamList = paramList;
        }

        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            if (Target != null)
            {
                Target.ToTextCode(nameMap, writer, indent);
                writer.Write(".");
            }
            if (InvokeSpecial && LibraryId == -2)
            {
                if (nameMap.MethodIdToClassId.TryGetValue(MethodId, out var classId) && EplSystemId.GetType(classId) == EplSystemId.Type_Class)
                {
                    writer.Write(nameMap.GetUserDefinedName(classId));
                    writer.Write(".");
                }
            }
            writer.Write(LibraryId == -2 || LibraryId == -3 ? nameMap.GetUserDefinedName(MethodId) : nameMap.GetLibCmdName(LibraryId, MethodId));
            writer.Write(" ");
            ParamList.ToTextCode(nameMap, writer, indent);
        }
        internal void WriteTo(MethodCodeDataWriterArgs a, byte type, bool mask, string comment)
        {
            if (Target != null) a.VariableReference.Write(a.Offest);
            if (LibraryId == -2 || LibraryId == -3) a.MethodReference.Write(a.Offest);
            a.ExpressionData.Write(type);
            a.ExpressionData.Write(MethodId);
            a.ExpressionData.Write(LibraryId);
            a.ExpressionData.Write((short)((mask ? 0x20 : 0) | (InvokeSpecial ? 0x10 : 0)));
            a.ExpressionData.WriteBStr(a.Encoding, null);
            a.ExpressionData.WriteBStr(a.Encoding, "".Equals(comment) ? null : comment);
            if (Target == null)
            {
                a.ExpressionData.Write((byte)0x36);
            }
            else
            {
                if (Target is In0x38Expression)
                {
                    a.ExpressionData.Write((byte)0x38);
                    ((In0x38Expression)Target).WriteTo(a, false);
                    a.ExpressionData.Write((byte)0x37);
                }
                else
                {
                    a.ExpressionData.Write((byte)0x38);
                    a.ExpressionData.Write(EplSystemId.Id_NaV);
                    a.ExpressionData.Write((byte)0x3A);
                    Target.WriteTo(a);
                    a.ExpressionData.Write((byte)0x37);
                }
            }
            if (ParamList != null)
                ParamList.WriteTo(a);
            else
                ParamListEnd.Instance.WriteTo(a);
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a) => WriteTo(a, 0x21, false, string.Empty);
    }
}
