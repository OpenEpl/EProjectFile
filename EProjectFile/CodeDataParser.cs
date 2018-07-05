using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile
{
    public static class CodeDataParser
    {
        internal static string AddPrefixInEachLine(string x, string c)//Debug用
        {
            if (string.IsNullOrEmpty(x))
            {
                return "";
            }
            if (x.EndsWith("\r\n"))
            {
                return c + x.Substring(0, x.Length - 2).Replace("\r\n", "\r\n" + c) + "\r\n";
            }
            else
            {
                return c + x.Replace("\r\n", "\r\n" + c);
            }
        }
        public static StatementBlock ParseStatementBlock(byte[] expressionData)
        {
            return ParseStatementBlock(expressionData, out var lineOffest, out var blockOffest);
        }
        public static StatementBlock ParseStatementBlock(byte[] expressionData, out byte[] lineOffest, out byte[] blockOffest)
        {
            if (expressionData == null)
            {
                throw new ArgumentNullException(nameof(expressionData));
            }

            BinaryWriter newWriter() => new BinaryWriter(new MemoryStream());
            byte[] getBytes(BinaryWriter x) => ((MemoryStream)x.BaseStream).ToArray();
            using (BinaryWriter lineOffestWriter = newWriter(), blockOffestWriter = newWriter())
            {
                var result = ParseStatementBlock(new BinaryReader(new MemoryStream(expressionData, false)), lineOffestWriter, blockOffestWriter);
                lineOffest = getBytes(lineOffestWriter);
                blockOffest = getBytes(blockOffestWriter);
                return result;
            }

        }
        private static StatementBlock ParseStatementBlock(BinaryReader reader, BinaryWriter lineOffestWriter, BinaryWriter blockOffestWriter)
        {
            var block = new StatementBlock();
            while (!(reader.BaseStream.Position == reader.BaseStream.Length))
            {
                var knownTypeId = new int[]
                {
                    0x50, // 否则
                    0x51, // 如果结束
                    0x52, // 如果真结束
                    0x53, // .判断 某Case结束
                    0x54, // .判断结束
                    0x55, // 循环体结束标识（0x71前）
                    0x6A, // 常规Call
                    0x6B, // 如果
                    0x6C, // 如果真
                    0x6D, // .判断开始（紧接着就是 0x6E）
                    0x6E, // .判断 某Case开始
                    0x6F, // .默认
                    0x70, // 循环开始语句：XX循环首(参数...)
                    0x71, // 循环结束语句：XX循环尾(参数...)
                };
                Array.Sort(knownTypeId);
                var type = reader.ReadByte();
                while (Array.BinarySearch(knownTypeId, type) < 0)
                {
                    //尝试跳过未知信息
                    type = reader.ReadByte();
                }
                var startOffest = (int)reader.BaseStream.Position - 1;//typeId到代码数据开头的偏移位置
                if (lineOffestWriter != null)
                {
                    if (true //部分数据不需要将位置写入LineOffest（一般为在IDE无显示的数据）
                        && type != 0x50 // 否则
                        && type != 0x51 // 如果结束
                        && type != 0x52 // 如果真结束
                        && type != 0x55 // 循环块结束标识：0x71前
                        && type != 0x54 // .判断结束
                        && type != 0x53 // .判断 某Case结束
                        && type != 0x6D // .判断开始（紧接着就是 0x6E）
                        && type != 0x6F // .默认
                        )
                    {
                        lineOffestWriter.Write(startOffest);
                    }
                }
                switch (type)
                {
                    case 0x50: // 否则
                    case 0x51: // 如果结束
                    case 0x52: // 如果真结束
                    case 0x53: // .判断 某Case结束
                    case 0x54: // .判断结束
                    case 0x6F: // .默认
                    case 0x71: // 循环结束语句：XX循环尾(参数...)
                        reader.BaseStream.Position = startOffest;
                        return block;
                    case 0x55: // 循环体结束标识（0x71前）
                        continue;
                    case 0x6D: // .判断开始（紧接着就是 0x6E）
                        {
                            blockOffestWriter.Write((byte)4);
                            blockOffestWriter.Write(startOffest);
                            long posToFillEndOffest = blockOffestWriter.BaseStream.Position;
                            blockOffestWriter.Write(0);
                            var s = new SwitchStatement();
                            if (reader.ReadByte() != 0x6E) // .判断 某Case开始
                            {
                                throw new Exception();
                            };
                            byte switch_type;
                            do
                            {
                                lineOffestWriter.Write((int)reader.BaseStream.Position - 1);
                                var caseInfo = new SwitchStatement.CaseInfo();
                                caseInfo.Condition = ParseCallExpressionWithoutType(reader, out caseInfo.UnexaminedCode, out caseInfo.Comment, out caseInfo.Mask).ParamList.ElementAtOrDefault(0);
                                caseInfo.Block = ParseStatementBlock(reader, lineOffestWriter, blockOffestWriter);
                                s.Case.Add(caseInfo);
                                if (reader.ReadByte() != 0x53)
                                {
                                    throw new Exception();
                                };
                            } while ((switch_type = reader.ReadByte()) == 0x6E);
                            if (switch_type != 0x6F) // .默认
                            {
                                throw new Exception();
                            };
                            s.DefaultBlock = ParseStatementBlock(reader, lineOffestWriter, blockOffestWriter);
                            if (reader.ReadByte() != 0x54) //.判断结束
                            {
                                throw new Exception();
                            };
                            int endOffest = (int)reader.BaseStream.Position;
                            blockOffestWriter.BaseStream.Position = posToFillEndOffest;
                            blockOffestWriter.Write(endOffest);
                            blockOffestWriter.BaseStream.Seek(0, SeekOrigin.End);
                            reader.ReadByte();//0x74
                            block.Add(s);
                        }
                        continue;
                }
                var exp = ParseCallExpressionWithoutType(reader, out string unexaminedCode, out string comment, out bool mask);
                switch (type)
                {
                    case 0x70: //循环开始语句：XX循环首(参数...)
                        {
                            blockOffestWriter.Write((byte)3);
                            blockOffestWriter.Write(startOffest);
                            long posToFillEndOffest = blockOffestWriter.BaseStream.Position;
                            blockOffestWriter.Write(0);

                            var loopblock = ParseStatementBlock(reader, lineOffestWriter, blockOffestWriter);
                            CallExpression endexp = null;

                            var endOffest = (int)reader.BaseStream.Position;
                            blockOffestWriter.BaseStream.Position = posToFillEndOffest;
                            blockOffestWriter.Write(endOffest);
                            blockOffestWriter.BaseStream.Seek(0, SeekOrigin.End);

                            string endexp_unexaminedCode;
                            string endexp_comment;
                            bool endexp_mask;
                            switch (reader.ReadByte())
                            {
                                case 0x71:
                                    endexp = ParseCallExpressionWithoutType(reader, out endexp_unexaminedCode, out endexp_comment, out endexp_mask);
                                    break;
                                default:
                                    throw new Exception();
                            }
                            if (exp.LibraryId != 0)
                            {
                                throw new Exception();
                            }
                            LoopStatement s = null;
                            switch (exp.MethodId)
                            {
                                case 3:
                                    s = new WhileStatement()
                                    {
                                        Condition = exp.ParamList.ElementAtOrDefault(0),
                                        Block = loopblock,
                                        UnexaminedCode = unexaminedCode
                                    };
                                    break;
                                case 5:
                                    s = new DoWhileStatement()
                                    {
                                        Condition = endexp.ParamList.ElementAtOrDefault(0),
                                        Block = loopblock,
                                        UnexaminedCode = endexp_unexaminedCode
                                    };
                                    break;
                                case 7:
                                    s = new CounterStatement()
                                    {
                                        Count = exp.ParamList.ElementAtOrDefault(0),
                                        Var = exp.ParamList.ElementAtOrDefault(1),
                                        Block = loopblock,
                                        UnexaminedCode = unexaminedCode
                                    };
                                    break;
                                case 9:
                                    s = new ForStatement()
                                    {
                                        Start = exp.ParamList.ElementAtOrDefault(0),
                                        End = exp.ParamList.ElementAtOrDefault(1),
                                        Step = exp.ParamList.ElementAtOrDefault(2),
                                        Var = exp.ParamList.ElementAtOrDefault(3),
                                        Block = loopblock,
                                        UnexaminedCode = unexaminedCode
                                    };
                                    break;
                                default:
                                    throw new Exception();
                            }

                            s.CommentOnStart = comment;
                            s.CommentOnEnd = endexp_comment;

                            s.MaskOnStart = mask;
                            s.MaskOnEnd = endexp_mask;

                            block.Add(s);
                        }
                        break;
                    case 0x6C: //如果真
                        {
                            blockOffestWriter.Write((byte)2);
                            blockOffestWriter.Write(startOffest);
                            long posToFillEndOffest = blockOffestWriter.BaseStream.Position;
                            blockOffestWriter.Write(0);

                            var s = new IfStatement()
                            {
                                Condition = exp.ParamList.ElementAtOrDefault(0),
                                UnexaminedCode = unexaminedCode,
                                Block = ParseStatementBlock(reader, lineOffestWriter, blockOffestWriter),
                                Comment = comment,
                                Mask = mask
                            };
                            if (reader.ReadByte() != 0x52)
                            {
                                throw new Exception();
                            };

                            var endOffest = (int)reader.BaseStream.Position;
                            blockOffestWriter.BaseStream.Position = posToFillEndOffest;
                            blockOffestWriter.Write(endOffest);
                            blockOffestWriter.BaseStream.Seek(0, SeekOrigin.End);

                            reader.ReadByte();//0x73

                            block.Add(s);
                        }
                        break;
                    case 0x6B: //如果
                        {
                            var s = new IfElseStatement()
                            {
                                Condition = exp.ParamList.ElementAtOrDefault(0),
                                UnexaminedCode = unexaminedCode,
                                Comment = comment,
                                Mask = mask
                            };
                            blockOffestWriter.Write((byte)1);
                            blockOffestWriter.Write(startOffest);
                            long posToFillEndOffest = blockOffestWriter.BaseStream.Position;
                            blockOffestWriter.Write(0);

                            s.BlockOnTrue = ParseStatementBlock(reader, lineOffestWriter, blockOffestWriter);
                            if (reader.ReadByte() != 0x50)
                            {
                                throw new Exception();
                            };
                            s.BlockOnFalse = ParseStatementBlock(reader, lineOffestWriter, blockOffestWriter);
                            if (reader.ReadByte() != 0x51)
                            {
                                throw new Exception();
                            };
                            var endOffest = (int)reader.BaseStream.Position;

                            blockOffestWriter.BaseStream.Position = posToFillEndOffest;
                            blockOffestWriter.Write(endOffest);
                            blockOffestWriter.BaseStream.Seek(0, SeekOrigin.End);

                            reader.ReadByte();//0x72

                            block.Add(s);
                        }
                        break;
                    case 0x6A: // 常规Call
                        {
                            if (unexaminedCode != null)
                            {
                                block.Add(new UnexaminedStatement()
                                {
                                    UnexaminedCode = unexaminedCode,
                                    Mask = mask
                                });
                            }
                            else
                            {
                                if (exp.LibraryId == -1)
                                {
                                    block.Add(new ExpressionStatement()
                                    {
                                        Expression = null,
                                        Comment = comment
                                    });
                                }
                                else
                                {
                                    block.Add(new ExpressionStatement()
                                    {
                                        Expression = exp,
                                        Comment = comment,
                                        Mask = mask
                                    });
                                }
                            }
                        }
                        break;
                }
            }
            return block;
        }
        private static Expression ParseExpression(BinaryReader reader, bool parseMember = true)
        {
            Expression result = null;
            byte type;
            while (true)
            {
                type = reader.ReadByte();
                switch (type)
                {
                    case 0x01:
                        result = ParamListEnd.Instance;
                        break;
                    case 0x16:
                        result = DefaultValueExpression.Instance;
                        break;
                    case 0x17:
                        result = new NumberLiteral(reader.ReadDouble());
                        break;
                    case 0x18:
                        result = BoolLiteral.ValueOf(reader.ReadInt16() != 0);
                        break;
                    case 0x19:
                        result = new DateTimeLiteral(DateTime.FromOADate(reader.ReadDouble()));
                        break;
                    case 0x1A:
                        result = new StringLiteral(reader.ReadBStr());
                        break;
                    case 0x1B:
                        result = new ConstantExpression(reader.ReadInt32());
                        break;
                    case 0x1C:
                        result = new ConstantExpression((short)(reader.ReadInt16() - 1), (short)(reader.ReadInt16() - 1));
                        break;
                    case 0x1D:
                        //0x1D 0x38 <Int32:VarId>
                        continue;
                    case 0x1E:
                        result = new MethodPtrExpression(reader.ReadInt32());
                        break;
                    case 0x21:
                        result = ParseCallExpressionWithoutType(reader);
                        break;
                    case 0x23:
                        result = new EmnuConstantExpression((short)(reader.ReadInt16() - 1), (short)(reader.ReadInt16() - 1), reader.ReadInt32());
                        break;
                    case 0x37:
                        continue;
                    case 0x1F:
                        {
                            var array = new ArrayLiteralExpression();
                            Expression exp;
                            while (!((exp = ParseExpression(reader)) is ArrayLiteralEnd))
                            {
                                array.Add(exp);
                            };
                            result = array;
                        }
                        break;
                    case 0x20:
                        result = ArrayLiteralEnd.Instance;
                        break;
                    case 0x38://ThisCall Or 访问变量
                        {
                            int variable = reader.ReadInt32();
                            if (variable == 0x0500FFFE)
                            {
                                reader.ReadByte(); //0x3A
                                return ParseExpression(reader, true);
                            }
                            else
                            {
                                result = new VariableExpression(variable);
                                parseMember = true;
                            }
                        }
                        break;
                    case 0x3B:
                        result = new NumberLiteral(reader.ReadInt32());
                        break;
                    default:
                        throw new Exception($"Unknown Type: {type.ToString("X2")}");
                }
                break;
            }
            if (parseMember
                && (result is VariableExpression
                || result is CallExpression
                || result is AccessMemberExpression
                || result is AccessArrayExpression))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    switch (reader.ReadByte())
                    {
                        case 0x39:
                            int MemberId = reader.ReadInt32();
                            if(EplSystemId.GetType(MemberId) == EplSystemId.Type_StructMember)
                                result = new AccessMemberExpression(result, reader.ReadInt32(), MemberId);
                            else
                                result = new AccessMemberExpression(result, (short)(reader.ReadInt16() - 1), (short)(reader.ReadInt16() - 1), MemberId);
                            break;
                        case 0x3A:
                            result = new AccessArrayExpression(result, ParseExpression(reader, false));
                            break;
                        case 0x37:
                            goto parse_member_finish;
                        default:
                            reader.BaseStream.Position -= 1;
                            goto parse_member_finish;
                    }
                }
            }
            parse_member_finish:
            return result;
        }

        private static ParamListExpression ParseParamList(BinaryReader reader)
        {
            var param = new ParamListExpression();
            Expression exp;
            while (!((exp = ParseExpression(reader)) is ParamListEnd))
            {
                param.Add(exp);
            };
            return param;
        }
        private static CallExpression ParseCallExpressionWithoutType(BinaryReader reader)
        {
            var exp = ParseCallExpressionWithoutType(reader, out string unexaminedCode, out string comment, out bool mask);
            return exp;
        }

        private static CallExpression ParseCallExpressionWithoutType(BinaryReader reader, out string unexaminedCode, out string comment, out bool mask)
        {
            var methodId = reader.ReadInt32();
            var libraryId = reader.ReadInt16();
            var flag = reader.ReadInt16();
            unexaminedCode = reader.ReadBStr();
            comment = reader.ReadBStr();
            mask = (flag & 0x20) != 0;
            //bool expand = (flag & 0x1) != 0;
            if (unexaminedCode != null)
            {
                int nullPos = unexaminedCode.IndexOf('\0');
                if (nullPos != -1) unexaminedCode = unexaminedCode.Substring(0, nullPos);
                if ("".Equals(unexaminedCode)) unexaminedCode = null;
            }
            if (comment != null)
            {
                int nullPos = comment.IndexOf('\0');
                if (nullPos != -1) comment = comment.Substring(0, nullPos);
                if ("".Equals(comment)) comment = null;
            }
            var exp = new CallExpression(libraryId, methodId);
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                switch (reader.ReadByte())
                {
                    case 0x36:
                        exp.ParamList = ParseParamList(reader);
                        break;
                    case 0x38://ThisCall
                        reader.BaseStream.Position -= 1;
                        exp.Target = ParseExpression(reader);
                        exp.ParamList = ParseParamList(reader);
                        break;
                    default:
                        reader.BaseStream.Position -= 1;
                        throw new Exception();
                }
            }
            return exp;
        }
    }


    internal struct MethodCodeDataWriterArgs
    {
        public BinaryWriter LineOffest;
        public BinaryWriter BlockOffest;
        public BinaryWriter MethodReference;
        public BinaryWriter VariableReference;
        public BinaryWriter ConstantReference;
        public BinaryWriter ExpressionData;
        public int Offest => (int)ExpressionData.BaseStream.Position;
        public IDisposable NewBlock(byte type)
        {
            return new BlockOffestHelper(this, type);
        }
        private struct BlockOffestHelper : IDisposable
        {
            private bool Disposed;
            private MethodCodeDataWriterArgs a;
            private long posToFillEndOffest;
            public BlockOffestHelper(MethodCodeDataWriterArgs writers, byte type)
            {
                Disposed = false;
                a = writers;
                a.BlockOffest.Write(type);
                a.BlockOffest.Write(a.Offest);
                posToFillEndOffest = a.BlockOffest.BaseStream.Position;
                a.BlockOffest.Write(0);
            }
            public void Dispose()
            {
                if (Disposed) return; else Disposed = true;
                long curPos = a.BlockOffest.BaseStream.Position;
                a.BlockOffest.BaseStream.Position = posToFillEndOffest;
                a.BlockOffest.Write(a.Offest);
                a.BlockOffest.BaseStream.Position = curPos;
            }
        }

    }

    /// <summary>
    /// 表达式 基类
    /// </summary>
    public abstract class Expression
    {
        internal abstract void WriteTo(MethodCodeDataWriterArgs a);
        public abstract void ToTextCode(IDToNameMap nameMap,StringBuilder result);
        public string ToString(IDToNameMap nameMap)
        {
            var builder = new StringBuilder();
            ToTextCode(nameMap, builder);
            return builder.ToString();
        }
        public sealed override string ToString() => "[请使用ToString(IDToNameMap)代替ToString()]" + base.ToString();
    }
    /// <summary>
    /// 解析时临时标记（参数列表结束标识）
    /// </summary>
    internal class ParamListEnd : Expression
    {
        public static readonly ParamListEnd Instance = new ParamListEnd();
        private ParamListEnd()
        {
            if (Instance != null) throw new NotSupportedException();
        }

        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result) => throw new NotImplementedException();

        internal override void WriteTo(MethodCodeDataWriterArgs a) => a.ExpressionData.Write((byte)0x01);
    }
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
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result) => throw new NotImplementedException();

        internal override void WriteTo(MethodCodeDataWriterArgs a) => a.ExpressionData.Write((byte)0x20);
    }
    public class DefaultValueExpression : Expression
    {
        public static readonly DefaultValueExpression Instance = new DefaultValueExpression();
        private DefaultValueExpression()
        {
            if (Instance != null) throw new NotSupportedException();
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a) => a.ExpressionData.Write((byte)0x16);
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            //Nothing need doing.
        }
    }
    /// <summary>
    /// 语句 基类
    /// </summary>
    public abstract class Statement
    {
        internal abstract void WriteTo(MethodCodeDataWriterArgs a);
        /// <summary>
        /// 开头含缩进，结尾不含换行
        /// </summary>
        /// <param name="nameMap"></param>
        /// <param name="result"></param>
        /// <param name="indent"></param>
        public abstract void ToTextCode(IDToNameMap nameMap, StringBuilder result,int indent = 0);
        public string ToString(IDToNameMap nameMap)
        {
            var builder = new StringBuilder();
            ToTextCode(nameMap, builder);
            return builder.ToString();
        }
        public sealed override string ToString() => "[请使用ToString(IDToNameMap)代替ToString()]" + base.ToString();
    }
    /// <summary>
    /// 表达式语句
    /// </summary>
    public class ExpressionStatement : Statement
    {
        public CallExpression Expression;
        public bool Mask;

        public string Comment;
        public ExpressionStatement()
        {

        }

        public ExpressionStatement(CallExpression expression, bool mask, string comment)
        {
            Expression = expression;
            Mask = mask;
            Comment = comment;
        }

        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (Mask)
                result.Append("' ");
            Expression?.ToTextCode(nameMap, result);
            if (Comment != null) 
            {
                result.Append("' ");
                result.Append(Comment);
            }
        }

        internal void WriteTo(MethodCodeDataWriterArgs a, byte type)
        {
            a.LineOffest.Write(a.Offest);
            if (Expression != null)
            {
                Expression.WriteTo(a, type, Mask, Comment);
            }
            else
            {
                new CallExpression(-1, 0).WriteTo(a, type, Mask, Comment);
            }
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            WriteTo(a, 0x6A);
        }
    }
    /// <summary>
    /// 未验证代码语句
    /// </summary>
    public class UnexaminedStatement : Statement
    {
        private string unexaminedCode;
        public bool Mask;

        public string UnexaminedCode { get => unexaminedCode; set => unexaminedCode = value ?? throw new ArgumentNullException(nameof(UnexaminedCode)); }

        public UnexaminedStatement()
        {
        }

        public UnexaminedStatement(string unexaminedCode, bool mask)
        {
            UnexaminedCode = unexaminedCode;
            Mask = mask;
        }
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (Mask)
                result.Append("' ");
            result.Append(unexaminedCode);
        }
        internal void WriteTo(MethodCodeDataWriterArgs a, byte type)
        {
            a.LineOffest.Write(a.Offest);
            a.ExpressionData.Write(type);
            a.ExpressionData.Write(0);
            a.ExpressionData.Write((short)-1);
            a.ExpressionData.Write(Mask);
            a.ExpressionData.WriteBStr(UnexaminedCode);
            a.ExpressionData.WriteBStr(null);
            a.ExpressionData.Write((byte)0x36);
            ParamListEnd.Instance.WriteTo(a);

        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            WriteTo(a, 0x6A);

        }
    }
    /// <summary>
    /// 如果 语句块
    /// </summary>
    public class IfElseStatement : Statement
    {
        public Expression Condition;
        public string UnexaminedCode;//UnexaminedCode!=null时Condition==null
        public StatementBlock BlockOnTrue;
        public StatementBlock BlockOnFalse;
        public string Comment;
        public bool Mask;

        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (Mask)
                result.Append("' ");
            if (UnexaminedCode == null)
            {
                result.Append(".如果 (");
                Condition.ToTextCode(nameMap, result);
                result.Append(")");
            }
            else
            {
                result.Append(".");
                result.Append(UnexaminedCode);
            }
            if (Comment != null)
            {
                result.Append("' ");
                result.Append(Comment);
            }
            result.AppendLine();
            BlockOnTrue.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            result.AppendLine(".否则");
            BlockOnFalse.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            result.Append(".如果结束");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(1))
            {
                if (UnexaminedCode != null)
                    new UnexaminedStatement(UnexaminedCode, Mask).WriteTo(a, 0x6B);
                else
                    new ExpressionStatement(new CallExpression(0, 0, new ParamListExpression() { Condition }), Mask, Comment).WriteTo(a, 0x6B);
                BlockOnTrue.WriteTo(a);
                a.ExpressionData.Write((byte)0x50);
                BlockOnFalse.WriteTo(a);
                a.ExpressionData.Write((byte)0x51);
            }
            a.ExpressionData.Write((byte)0x72);
        }
    }
    /// <summary>
    /// 如果真 语句块
    /// </summary>
    public class IfStatement : Statement
    {
        public Expression Condition;
        public string UnexaminedCode;//UnexaminedCode!=null时Condition==null
        public StatementBlock Block;
        public string Comment;
        public bool Mask;
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (Mask)
                result.Append("' ");
            if (UnexaminedCode == null)
            {
                result.Append(".如果真 (");
                Condition.ToTextCode(nameMap, result);
                result.Append(")");
            }
            else
            {
                result.Append(".");
                result.Append(UnexaminedCode);
            }
            if (Comment != null)
            {
                result.Append("' ");
                result.Append(Comment);
            }
            result.AppendLine();
            Block.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            result.Append(".如果真结束");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(2))
            {
                if (UnexaminedCode != null)
                    new UnexaminedStatement(UnexaminedCode, Mask).WriteTo(a, 0x6C);
                else
                    new ExpressionStatement(new CallExpression(0, 1, new ParamListExpression() { Condition }), Mask, Comment).WriteTo(a, 0x6C);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x52);
            }
            a.ExpressionData.Write((byte)0x73);
        }
    }
    /// <summary>
    /// 循环语句块 基类
    /// </summary>
    public abstract class LoopStatement : Statement
    {
        public StatementBlock Block;
        public string UnexaminedCode;//UnexaminedCode!=null其他循环参数为null
        public string CommentOnStart;
        public string CommentOnEnd;
        public bool MaskOnStart;
        public bool MaskOnEnd;
    }
    /// <summary>
    /// 判断循环 语句块
    /// </summary>
    public class WhileStatement : LoopStatement
    {
        public Expression Condition;
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnStart)
                result.Append("' ");
            if (UnexaminedCode == null)
            {
                result.Append(".判断循环首 (");
                Condition.ToTextCode(nameMap, result);
                result.Append(")");
            }
            else
            {
                result.Append(".");
                result.Append(UnexaminedCode);
            }
            if (CommentOnStart != null)
            {
                result.Append("' ");
                result.Append(CommentOnStart);
            }
            result.AppendLine();
            Block.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnEnd)
                result.Append("' ");
            result.Append(".判断循环尾 ()");
            if (CommentOnEnd != null)
            {
                result.Append("' ");
                result.Append(CommentOnEnd);
            }
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(3))
            {
                if (UnexaminedCode != null)
                    new UnexaminedStatement(UnexaminedCode, MaskOnStart).WriteTo(a, 0x70);
                else
                    new ExpressionStatement(new CallExpression(0, 3, new ParamListExpression() { Condition }), MaskOnStart, CommentOnStart).WriteTo(a, 0x70);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x55);
            }
            new ExpressionStatement(new CallExpression(0, 4, new ParamListExpression() { }), MaskOnEnd, CommentOnEnd).WriteTo(a, 0x71);
        }
    }
    /// <summary>
    /// 循环判断 语句块
    /// </summary>
    public class DoWhileStatement : LoopStatement
    {
        public Expression Condition;
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnStart)
                result.Append("' ");
            result.Append(".判断循环首 ()");
            if (CommentOnStart != null)
            {
                result.Append("' ");
                result.Append(CommentOnStart);
            }
            result.AppendLine();
            Block.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnEnd)
                result.Append("' ");
            if (UnexaminedCode == null)
            {
                result.Append(".判断循环尾 (");
                Condition.ToTextCode(nameMap, result);
                result.Append(")");
            }
            else
            {
                result.Append(".");
                result.Append(UnexaminedCode);
            }
            if (CommentOnEnd != null)
            {
                result.Append("' ");
                result.Append(CommentOnEnd);
            }
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(3))
            {
                new ExpressionStatement(new CallExpression(0, 6, new ParamListExpression() { }), MaskOnStart, CommentOnStart).WriteTo(a, 0x70);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x55);
            }
            if (UnexaminedCode != null)
                new UnexaminedStatement(UnexaminedCode, MaskOnEnd).WriteTo(a, 0x71);
            else
                new ExpressionStatement(new CallExpression(0, 5, new ParamListExpression() { Condition }), MaskOnEnd, CommentOnEnd).WriteTo(a, 0x71);
        }
    }
    /// <summary>
    /// 计次循环 语句块
    /// </summary>
    public class CounterStatement : LoopStatement
    {
        public Expression Count;
        public Expression Var;
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnStart)
                result.Append("' ");
            if (UnexaminedCode == null)
            {
                result.Append(".计次循环首 (");
                Count.ToTextCode(nameMap, result);
                result.Append(", ");
                Var.ToTextCode(nameMap, result);
                result.Append(")");
            }
            else
            {
                result.Append(".");
                result.Append(UnexaminedCode);
            }
            if (CommentOnStart != null)
            {
                result.Append("' ");
                result.Append(CommentOnStart);
            }
            result.AppendLine();
            Block.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnEnd)
                result.Append("' ");
            result.Append(".计次循环尾 ()");
            if (CommentOnEnd != null)
            {
                result.Append("' ");
                result.Append(CommentOnEnd);
            }
        }

        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(3))
            {
                if (UnexaminedCode != null)
                    new UnexaminedStatement(UnexaminedCode, MaskOnStart).WriteTo(a, 0x70);
                else
                    new ExpressionStatement(new CallExpression(0, 7, new ParamListExpression() { Count, Var }), MaskOnStart, CommentOnStart).WriteTo(a, 0x70);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x55);
            }
            new ExpressionStatement(new CallExpression(0, 8, new ParamListExpression() { }), MaskOnEnd, CommentOnEnd).WriteTo(a, 0x71);
        }
    }
    /// <summary>
    /// 变量循环 语句块
    /// </summary>
    public class ForStatement : LoopStatement
    {
        public Expression Start;
        public Expression End;
        public Expression Step;
        public Expression Var;
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnStart)
                result.Append("' ");
            if (UnexaminedCode == null)
            {
                result.Append(".变量循环首 (");
                Start.ToTextCode(nameMap, result);
                result.Append(", ");
                End.ToTextCode(nameMap, result);
                result.Append(", ");
                Step.ToTextCode(nameMap, result);
                result.Append(", ");
                Var.ToTextCode(nameMap, result);
                result.Append(")");
            }
            else
            {
                result.Append(".");
                result.Append(UnexaminedCode);
            }
            if (CommentOnStart != null)
            {
                result.Append("' ");
                result.Append(CommentOnStart);
            }
            result.AppendLine();
            Block.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (MaskOnEnd)
                result.Append("' ");
            result.Append(".变量循环尾 ()");
            if (CommentOnEnd != null)
            {
                result.Append("' ");
                result.Append(CommentOnEnd);
            }
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(3))
            {
                if (UnexaminedCode != null)
                    new UnexaminedStatement(UnexaminedCode, MaskOnStart).WriteTo(a, 0x70);
                else
                    new ExpressionStatement(new CallExpression(0, 9, new ParamListExpression() { Start, End, Step, Var }), MaskOnStart, CommentOnStart).WriteTo(a, 0x70);
                Block.WriteTo(a);
                a.ExpressionData.Write((byte)0x55);
            }
            new ExpressionStatement(new CallExpression(0, 10, new ParamListExpression() { }), MaskOnEnd, CommentOnEnd).WriteTo(a, 0x71);
        }
    }
    /// <summary>
    /// 判断 语句块
    /// </summary>
    public class SwitchStatement : Statement
    {
        public int StartOffest;
        public int EndOffest;
        public class CaseInfo
        {
            public Expression Condition;
            public string UnexaminedCode;
            public StatementBlock Block;
            public string Comment;
            public bool Mask;
        }
        public List<CaseInfo> Case = new List<CaseInfo>();
        public StatementBlock DefaultBlock;
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            if (Case.Count == 0)
                throw new Exception("Must hava a case");
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            if (Case[0].Mask)
                result.Append("' ");
            if (Case[0].UnexaminedCode == null)
            {
                result.Append(".判断开始 (");
                Case[0].Condition.ToTextCode(nameMap, result);
                result.Append(")");
            }
            else
            {
                result.Append(".判断开始");
                result.Append(Case[0].UnexaminedCode.TrimStart().Substring("判断".Length));
            }
            if (Case[0].Comment != null)
            {
                result.Append("' ");
                result.Append(Case[0].Comment);
            }
            result.AppendLine();
            Case[0].Block.ToTextCode(nameMap, result, indent + 1);
            for (int i = 1; i < Case.Count; i++)
            {
                result.AppendLine();
                for (int x = 0; x < indent; x++)
                    result.Append("    ");
                if (Case[i].Mask)
                    result.Append("' ");
                if (Case[i].UnexaminedCode == null)
                {
                    result.Append(".判断 (");
                    Case[i].Condition.ToTextCode(nameMap, result);
                    result.Append(")");
                }
                else
                {
                    result.Append(".");
                    result.Append(Case[i].UnexaminedCode);
                }
                if (Case[i].Comment != null)
                {
                    result.Append("' ");
                    result.Append(Case[i].Comment);
                }
                result.AppendLine();
                Case[i].Block.ToTextCode(nameMap, result, indent + 1);
            }
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            result.AppendLine(".默认");
            DefaultBlock.ToTextCode(nameMap, result, indent + 1);
            result.AppendLine();
            for (int i = 0; i < indent; i++)
                result.Append("    ");
            result.Append(".判断结束");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            using (a.NewBlock(4))
            {
                a.ExpressionData.Write((byte)0x6D);
                foreach (var curCase in Case)
                {
                    if (curCase.UnexaminedCode != null)
                        new UnexaminedStatement(curCase.UnexaminedCode, curCase.Mask).WriteTo(a, 0x6E);
                    else
                        new ExpressionStatement(new CallExpression(0, 2, new ParamListExpression() { curCase.Condition }), curCase.Mask, curCase.Comment).WriteTo(a, 0x6E);
                    curCase.Block.WriteTo(a);
                    a.ExpressionData.Write((byte)0x53);
                }
                a.ExpressionData.Write((byte)0x6F);
                DefaultBlock.WriteTo(a);
                a.ExpressionData.Write((byte)0x54);
            }
            a.ExpressionData.Write((byte)0x74);
        }
    }
    /// <summary>
    /// 语句块
    /// </summary>
    public class StatementBlock : IList<Statement>
    {
        private List<Statement> Statements = new List<Statement>();

        public int Count => ((IList<Statement>)Statements).Count;

        public bool IsReadOnly => ((IList<Statement>)Statements).IsReadOnly;

        public Statement this[int index] { get => ((IList<Statement>)Statements)[index]; set => ((IList<Statement>)Statements)[index] = value; }

        public StatementBlock()
        {

        }
        internal void WriteTo(MethodCodeDataWriterArgs a)
        {
            Statements.ForEach(x => x.WriteTo(a));
        }
        public MethodCodeData ToCodeData()
        {
            BinaryWriter newWriter() => new BinaryWriter(new MemoryStream());
            byte[] getBytes(BinaryWriter x) => ((MemoryStream)x.BaseStream).ToArray();
            using (BinaryWriter
                lineOffest = newWriter(),
                blockOffest = newWriter(),
                methodReference = newWriter(),
                variableReference = newWriter(),
                constantReference = newWriter(),
                expressionData = newWriter())
            {
                var a = new MethodCodeDataWriterArgs
                {
                    LineOffest = lineOffest,
                    BlockOffest = blockOffest,
                    MethodReference = methodReference,
                    VariableReference = variableReference,
                    ConstantReference = constantReference,
                    ExpressionData = expressionData
                };
                WriteTo(a);
                return new MethodCodeData
                {
                    LineOffest = getBytes(lineOffest),
                    BlockOffest = getBytes(blockOffest),
                    MethodReference = getBytes(methodReference),
                    VariableReference = getBytes(variableReference),
                    ConstantReference = getBytes(constantReference),
                    ExpressionData = getBytes(expressionData)
                };
            }
        }
        public void ToTextCode(IDToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            for (int i = 0; i < Count; i++)
            {
                if (i != 0)
                    result.AppendLine();
                this[i].ToTextCode(nameMap, result, indent);
            }
        }
        public string ToString(IDToNameMap nameMap)
        {
            var builder = new StringBuilder();
            ToTextCode(nameMap, builder);
            return builder.ToString();
        }
        public sealed override string ToString() => "[请使用ToString(IDToNameMap)代替ToString()]" + base.ToString();

        public int IndexOf(Statement item)
        {
            return ((IList<Statement>)Statements).IndexOf(item);
        }

        public void Insert(int index, Statement item)
        {
            ((IList<Statement>)Statements).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Statement>)Statements).RemoveAt(index);
        }

        public void Add(Statement item)
        {
            ((IList<Statement>)Statements).Add(item);
        }

        public void Clear()
        {
            ((IList<Statement>)Statements).Clear();
        }

        public bool Contains(Statement item)
        {
            return ((IList<Statement>)Statements).Contains(item);
        }

        public void CopyTo(Statement[] array, int arrayIndex)
        {
            ((IList<Statement>)Statements).CopyTo(array, arrayIndex);
        }

        public bool Remove(Statement item)
        {
            return ((IList<Statement>)Statements).Remove(item);
        }

        public IEnumerator<Statement> GetEnumerator()
        {
            return ((IList<Statement>)Statements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Statement>)Statements).GetEnumerator();
        }
    }
    /// <summary>
    /// 访问常量
    /// </summary>
    public class ConstantExpression : Expression
    {
        public readonly short LibraryId;
        public readonly int ConstantId;
        public ConstantExpression(short LibraryId, int ConstantId)
        {
            this.LibraryId = LibraryId;
            this.ConstantId = ConstantId;
        }
        public ConstantExpression(int ConstantId)
        {
            this.LibraryId = -2;
            this.ConstantId = ConstantId;
        }
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append("#");
            result.Append(LibraryId == -2 ? nameMap.GetUserDefinedName(ConstantId):nameMap.GetLibConstantName(LibraryId,ConstantId));
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
    /// <summary>
    /// 访问枚举（支持库）
    /// </summary>
    public class EmnuConstantExpression : Expression
    {
        public readonly short LibraryId;
        public readonly short StructId;
        public readonly int MemberId;
        public EmnuConstantExpression(short StructId, short LibraryId, int MemberId)
        {
            this.StructId = StructId;
            this.LibraryId = LibraryId;
            this.MemberId = MemberId;
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x23);
            a.ExpressionData.Write((short)(StructId + 1));
            a.ExpressionData.Write((short)(LibraryId + 1));
            a.ExpressionData.Write(MemberId);
        }
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append("#");
            result.Append(nameMap.GetLibTypeName(LibraryId, StructId));
            result.Append(".");
            result.Append(nameMap.GetLibTypeMemberName(LibraryId, StructId, MemberId));
        }
    }

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
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append("&");
            result.Append(nameMap.GetUserDefinedName(MethodId));
        }
    }
    /// <summary>
    /// 常规子程序调用表达式
    /// </summary>
    public class CallExpression : Expression
    {
        public readonly short LibraryId;
        public readonly int MethodId;
        public Expression Target;//ThisCall
        public ParamListExpression ParamList = null;
        public CallExpression(short libraryId, int methodId, ParamListExpression paramList = null)
        {
            LibraryId = libraryId;
            MethodId = methodId;
            ParamList = paramList;
        }

        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            if (Target != null) 
            {
                Target.ToTextCode(nameMap, result);
                result.Append(".");
            }
            result.Append(LibraryId == -2 ? nameMap.GetUserDefinedName(MethodId) : nameMap.GetLibCmdName(LibraryId, MethodId));
            ParamList.ToTextCode(nameMap, result);
        }
        internal void WriteTo(MethodCodeDataWriterArgs a, byte type, bool mask, string comment)
        {
            if (Target != null) a.VariableReference.Write(a.Offest);
            if (LibraryId == -2 || LibraryId == -3) a.MethodReference.Write(a.Offest);
            a.ExpressionData.Write(type);
            a.ExpressionData.Write(MethodId);
            a.ExpressionData.Write(LibraryId);
            a.ExpressionData.Write((short)(mask ? 0x20 : 0));
            a.ExpressionData.WriteBStr(null);
            a.ExpressionData.WriteBStr("".Equals(comment) ? null : comment);
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
            if (ParamList != null) ParamList.WriteTo(a); else ParamListEnd.Instance.WriteTo(a);
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a) => WriteTo(a, 0x21, false, string.Empty);
    }
    /// <summary>
    /// 参数列表，配合CallExpression用
    /// </summary>
    public class ParamListExpression : Expression, IList<Expression>
    {
        private readonly List<Expression> Value = new List<Expression>();

        public int Count => Value.Count;

        public bool IsReadOnly => ((IList<Expression>)Value).IsReadOnly;

        public Expression this[int index] { get => Value[index]; set => Value[index] = value; }

        public void Add(Expression item)
        {
            Value.Add(item ?? DefaultValueExpression.Instance);
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            Value.ForEach(x => x.WriteTo(a));
            ParamListEnd.Instance.WriteTo(a);
        }
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append("(");
            for (int i = 0; i < Count; i++)
            {
                if (i != 0)
                    result.Append(", ");
                this[i].ToTextCode(nameMap, result);
            }
            result.Append(")");
        }

        public IEnumerator<Expression> GetEnumerator() => ((IEnumerable<Expression>)Value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Expression>)Value).GetEnumerator();

        public int IndexOf(Expression item)
        {
            return Value.IndexOf(item ?? DefaultValueExpression.Instance);
        }

        public void Insert(int index, Expression item)
        {
            Value.Insert(index, item ?? DefaultValueExpression.Instance);
        }

        public void RemoveAt(int index)
        {
            Value.RemoveAt(index);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(Expression item)
        {
            return Value.Contains(item ?? DefaultValueExpression.Instance);
        }

        public void CopyTo(Expression[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public bool Remove(Expression item)
        {
            return Value.Remove(item ?? DefaultValueExpression.Instance);
        }
    }
    /// <summary>
    /// 数组字面量（常量集），包括各种类型数组、字节集等
    /// </summary>
    public class ArrayLiteralExpression : Expression, IList<Expression>
    {
        private readonly List<Expression> Value = new List<Expression>();

        public Expression this[int index] { get => Value[index]; set => Value[index] = value; }

        public int Count => Value.Count;

        public bool IsReadOnly => ((IList<Expression>)Value).IsReadOnly;

        public void Add(Expression item)
        {
            Value.Add(item);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(Expression item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(Expression[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            return ((IList<Expression>)Value).GetEnumerator();
        }

        public int IndexOf(Expression item)
        {
            return Value.IndexOf(item);
        }

        public void Insert(int index, Expression item)
        {
            Value.Insert(index, item);
        }

        public bool Remove(Expression item)
        {
            return Value.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Value.RemoveAt(index);
        }
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append("{");
            for (int i = 0; i < Count; i++)
            {
                if (i != 0)
                    result.Append(", ");
                this[i].ToTextCode(nameMap, result);
            }
            result.Append("}");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x1F);
            Value.ForEach(x => x.WriteTo(a));
            ArrayLiteralEnd.Instance.WriteTo(a);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Expression>)Value).GetEnumerator();
        }
    }
    /// <summary>
    /// 日期时间型字面量
    /// </summary>
    public class DateTimeLiteral : Expression
    {
        public readonly DateTime Value;

        public DateTimeLiteral(DateTime value)
        {
            this.Value = value;
        }

        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append("[");
            if (Value != null)
            {
                if (Value.TimeOfDay.TotalSeconds == 0)
                    result.Append(Value.ToString("yyyy年MM月dd日"));
                else
                    result.Append(Value.ToString("yyyy年MM月dd日HH时mm分ss秒"));
            }
            result.Append("]");

        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x19);
            a.ExpressionData.Write(Value.ToOADate());
        }
    }
    /// <summary>
    /// 文本型字面量
    /// </summary>
    public class StringLiteral : Expression
    {
        public readonly String Value;
        public StringLiteral(String value)
        {
            this.Value = value;
        }
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append("“");
            result.Append(Value ?? "");
            result.Append("”");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x1A);
            a.ExpressionData.WriteBStr(Value ?? "");
        }
    }
    /// <summary>
    /// 数值型字面量（易语言内部统一按double处理）
    /// </summary>
    public class NumberLiteral : Expression
    {
        public readonly double Value;
        public NumberLiteral(double value)
        {
            this.Value = value;
        }
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append(Value);
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x17);
            a.ExpressionData.Write(Value);
        }
    }
    /// <summary>
    /// 逻辑型字面量
    /// </summary>
    public class BoolLiteral : Expression
    {
#pragma warning disable CS0612 // 类型或成员已过时
        public static readonly BoolLiteral True = new BoolLiteral(true);
        public static readonly BoolLiteral False = new BoolLiteral(false);
#pragma warning restore CS0612 // 类型或成员已过时
        public static BoolLiteral ValueOf(bool x) => x ? True : False;
        public readonly bool Value;
        [Obsolete]
        public BoolLiteral(bool value)
        {
            this.Value = value;
        }
        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append(Value ? "真" : "假");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x18);
            a.ExpressionData.Write((short)(Value ? -1 : 0));
        }
    }
    public abstract class In0x38Expression : Expression
    {
        internal abstract void WriteTo(MethodCodeDataWriterArgs a, bool need0x1DAnd0x37);
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            WriteTo(a, true);
        }
    }
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

        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            Target.ToTextCode(nameMap, result);
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
            if(LibraryId==-2)
                a.ExpressionData.Write(StructId);
            else
                a.ExpressionData.Write((StructId+1) & 0xFFFF | (LibraryId+1) << 16);
            if (need0x1DAnd0x37) a.ExpressionData.Write((byte)0x37);
        }
    }
    /// <summary>
    /// 访问变量表达式
    /// </summary>
    public class VariableExpression : In0x38Expression
    {
        public readonly int Id;
        public VariableExpression(int Id)
        {
            this.Id = Id;
        }

        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            result.Append(nameMap.GetUserDefinedName(Id));
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a, bool need0x1DAnd0x37)
        {
            if (need0x1DAnd0x37)
            {
                a.VariableReference.Write(a.Offest);
                a.ExpressionData.Write((byte)0x1D);
                a.ExpressionData.Write((byte)0x38);
            }
            a.ExpressionData.Write(Id);
            if (need0x1DAnd0x37) a.ExpressionData.Write((byte)0x37);
        }
    }
    /// <summary>
    /// 访问数组成员表达式，多维数组通过多个AccessArrayExpression嵌套表示
    /// </summary>
    public class AccessArrayExpression : In0x38Expression
    {
        public readonly Expression Target;
        public readonly Expression Index;
        public AccessArrayExpression(Expression target, Expression index)
        {
            this.Target = target;
            this.Index = index;
        }

        public override void ToTextCode(IDToNameMap nameMap, StringBuilder result)
        {
            Target.ToTextCode(nameMap, result);
            result.Append("[");
            Index.ToTextCode(nameMap, result);
            result.Append("]");
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
            a.ExpressionData.Write((byte)0x3A);
            if (Index is NumberLiteral)
            {
                a.ExpressionData.Write((byte)0x3B);
                a.ExpressionData.Write((int)((NumberLiteral)Index).Value);
            }
            else if (Index is In0x38Expression)
            {
                a.ExpressionData.Write((byte)0x38);
                ((In0x38Expression)Index).WriteTo(a, false);
                a.ExpressionData.Write((byte)0x37);
            }
            else
            {
                Index.WriteTo(a);
            }
            if (need0x1DAnd0x37) a.ExpressionData.Write((byte)0x37);
        }
    }
}
