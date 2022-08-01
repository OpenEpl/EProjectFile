using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 常规子程序调用表达式
    /// </summary>
    public class CallExpression : Expression
    {
        public enum OperatorType
        {
            Unary,
            Binary,
            Multi
        }
        public struct OperatorInfo
        {
            public readonly string Operator;
            /// <summary>
            /// 数值越小越优先
            /// </summary>
            public readonly int Precedence;
            public readonly OperatorType Type;

            public OperatorInfo(string @operator, int precedence, OperatorType type)
            {
                Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
                Precedence = precedence;
                Type = type;
            }
        }
        private static readonly Dictionary<int, OperatorInfo> OperatorMap = new Dictionary<int, OperatorInfo>()
        {
            { 15, new OperatorInfo("*", 2, OperatorType.Multi) },
            { 16, new OperatorInfo("/", 2, OperatorType.Multi) },
            { 17, new OperatorInfo("\\", 3, OperatorType.Multi) },
            { 18, new OperatorInfo("%", 4, OperatorType.Multi) },
            { 19, new OperatorInfo("+", 5, OperatorType.Multi) },
            { 20, new OperatorInfo("-", 5, OperatorType.Multi) /* 相减 */},
            { 21, new OperatorInfo("-", 1, OperatorType.Unary) /* 负*/ },
            { 38, new OperatorInfo("==", 6, OperatorType.Binary) /* 等于 */},
            { 39, new OperatorInfo("!=", 6, OperatorType.Binary) },
            { 40, new OperatorInfo("<", 6, OperatorType.Binary) },
            { 41, new OperatorInfo(">", 6, OperatorType.Binary) },
            { 42, new OperatorInfo("<=", 6, OperatorType.Binary) },
            { 43, new OperatorInfo(">=", 6, OperatorType.Binary) },
            { 44, new OperatorInfo("?=", 6, OperatorType.Binary) },
            { 45, new OperatorInfo("&&", 7, OperatorType.Multi) },
            { 46, new OperatorInfo("||", 8, OperatorType.Multi) },
            { 52, new OperatorInfo("=", 9, OperatorType.Binary) /* 赋值 */},
        };

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
            ToTextCode(nameMap, writer, indent, int.MaxValue);
        }

        private static void InvokeToTextCodeWithPrecedence(Expression expression, IdToNameMap nameMap, TextWriter writer, int indent, int expectedLowestPrecedence)
        {
            if (expression is CallExpression callExpression)
            {
                callExpression.ToTextCode(nameMap, writer, indent, expectedLowestPrecedence);
            }
            else
            {
                expression.ToTextCode(nameMap, writer, indent);
            }
        }

        private void ToTextCodeForOperator(IdToNameMap nameMap, TextWriter writer, int indent, int expectedLowestPrecedence, OperatorInfo operatorInfo)
        {
            var addBrackets = operatorInfo.Precedence > expectedLowestPrecedence;
            if (addBrackets)
            {
                writer.Write("(");
            }
            switch (operatorInfo.Type)
            {
                case OperatorType.Unary:
                    {
                        writer.Write(operatorInfo.Operator);
                        var expression = ParamList?.FirstOrDefault();
                        if (expression is null)
                        {
                            break;
                        }
                        InvokeToTextCodeWithPrecedence(expression, nameMap, writer, indent, operatorInfo.Precedence);
                        break;
                    }
                case OperatorType.Binary:
                case OperatorType.Multi:
                    {
                        // 预期行为：
                        // 如果无参数（null 或 length == 0），则输出 " + " 这样的不完整代码
                        // 如果只有一个参数，则输出 "1 + " 这样的不完整代码
                        // 如果有两个及以上参数，输出正常代码 "1 + 2 + 3"
                        var enumerator = ParamList?.GetEnumerator();
                        if (enumerator?.MoveNext() != true)
                        {
                            writer.Write(" ");
                            writer.Write(operatorInfo.Operator);
                            writer.Write(" ");
                            break;
                        }
                        InvokeToTextCodeWithPrecedence(enumerator.Current, nameMap, writer, indent, operatorInfo.Precedence);
                        writer.Write(" ");
                        writer.Write(operatorInfo.Operator);
                        writer.Write(" ");
                        if (enumerator.MoveNext())
                        {
                            InvokeToTextCodeWithPrecedence(enumerator.Current, nameMap, writer, indent, operatorInfo.Precedence - 1);
                            while (enumerator.MoveNext())
                            {
                                writer.Write(" ");
                                writer.Write(operatorInfo.Operator);
                                writer.Write(" ");
                                InvokeToTextCodeWithPrecedence(enumerator.Current, nameMap, writer, indent, operatorInfo.Precedence - 1);
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception($"Unhandled operator type: {operatorInfo.Type}");
            }
            if (addBrackets)
            {
                writer.Write(")");
            }
        }

        public bool TryGetOperatorInfo(out OperatorInfo operatorInfo)
        {
            if (Target == null && LibraryId == 0)
            {
                return OperatorMap.TryGetValue(MethodId, out operatorInfo);
            }
            operatorInfo = default;
            return false;
        }

        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent, int expectedLowestPrecedence)
        {
            if (TryGetOperatorInfo(out var operatorInfo))
            {
                ToTextCodeForOperator(nameMap, writer, indent, expectedLowestPrecedence, operatorInfo);
                return;
            }
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
