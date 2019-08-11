using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QIQI.EProjectFile.Expressions;
using QIQI.EProjectFile.Statements;
namespace QIQI.EProjectFile
{
    public static class CodeDataParser
    {
        [Obsolete]
        public static StatementBlock ParseStatementBlock(byte[] expressionData)
        {
            return ParseStatementBlock(expressionData, Encoding.GetEncoding("gbk"));
        }
        public static StatementBlock ParseStatementBlock(byte[] expressionData, Encoding encoding)
        {
#pragma warning disable CS0612
            return ParseStatementBlock(expressionData, encoding, out var lineOffest, out var blockOffest);
#pragma warning restore CS0612
        }
        [Obsolete]
        public static StatementBlock ParseStatementBlock(byte[] expressionData, out byte[] lineOffest, out byte[] blockOffest)
        {
            return ParseStatementBlock(expressionData, Encoding.GetEncoding("gbk"), out lineOffest, out blockOffest);
        }
        /// <summary>
        /// 低版本的LineOffest/BlockOffest修复引擎将随时删除
        /// </summary>
        /// <param name="expressionData">代码表达式数据</param>
        /// <param name="encoding">编码</param>
        /// <param name="lineOffest">修复LineOffest的结果</param>
        /// <param name="blockOffest">修复BlockOffest的结果</param>
        /// <returns></returns>
        [Obsolete]
        public static StatementBlock ParseStatementBlock(byte[] expressionData, Encoding encoding, out byte[] lineOffest, out byte[] blockOffest)
        {
            if (expressionData == null)
            {
                throw new ArgumentNullException(nameof(expressionData));
            }

            BinaryWriter newWriter() => new BinaryWriter(new MemoryStream());
            byte[] getBytes(BinaryWriter x) => ((MemoryStream)x.BaseStream).ToArray();
            using (BinaryWriter lineOffestWriter = newWriter(), blockOffestWriter = newWriter())
            {
                var result = ParseStatementBlock(new BinaryReader(new MemoryStream(expressionData, false), encoding), encoding, lineOffestWriter, blockOffestWriter);
                lineOffest = getBytes(lineOffestWriter);
                blockOffest = getBytes(blockOffestWriter);
                return result;
            }

        }
        private static readonly int[] KnownTypeId = new int[]
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
        static CodeDataParser()
        {
            Array.Sort(KnownTypeId);
        }
        private static StatementBlock ParseStatementBlock(BinaryReader reader, Encoding encoding, BinaryWriter lineOffestWriter, BinaryWriter blockOffestWriter)
        {
            var block = new StatementBlock();
            while (!(reader.BaseStream.Position == reader.BaseStream.Length))
            {
                var type = reader.ReadByte();
                while (Array.BinarySearch(KnownTypeId, type) < 0)
                {
                    // 尝试跳过未知信息
                    type = reader.ReadByte();
                }
                var startOffest = (int)reader.BaseStream.Position - 1; // typeId到代码数据开头的偏移位置
                if (lineOffestWriter != null)
                {
                    if (true // 部分数据不需要将位置写入LineOffest（一般为在IDE无显示的数据）
                        && type != 0x50 // 否则
                        && type != 0x51 // 如果结束
                        && type != 0x52 // 如果真结束
                        && type != 0x55 // 循环块结束标识：0x71前
                        && type != 0x54 // .判断结束
                        && type != 0x53 // .判断 某Case结束
                        && type != 0x6D // .判断开始（紧接着就是 0x6E）
                        && type != 0x6F) // .默认)
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
                            }
                            byte switch_type;
                            do
                            {
                                lineOffestWriter.Write((int)reader.BaseStream.Position - 1);
                                var caseInfo = new SwitchStatement.CaseInfo();
                                caseInfo.Condition = ParseCallExpressionWithoutType(reader, encoding, out var caseInfo_UnexaminedCode, out var caseInfo_Comment, out var caseInfo_Mask).ParamList.ElementAtOrDefault(0);
                                caseInfo.UnexaminedCode = caseInfo_UnexaminedCode;
                                caseInfo.Comment = caseInfo_Comment;
                                caseInfo.Mask = caseInfo_Mask;
                                caseInfo.Block = ParseStatementBlock(reader, encoding, lineOffestWriter, blockOffestWriter);
                                s.Case.Add(caseInfo);
                                if (reader.ReadByte() != 0x53)
                                {
                                    throw new Exception();
                                }
                            } while ((switch_type = reader.ReadByte()) == 0x6E);
                            if (switch_type != 0x6F) // .默认
                            {
                                throw new Exception();
                            }
                            s.DefaultBlock = ParseStatementBlock(reader, encoding, lineOffestWriter, blockOffestWriter);
                            if (reader.ReadByte() != 0x54) // .判断结束
                            {
                                throw new Exception();
                            }
                            int endOffest = (int)reader.BaseStream.Position;
                            blockOffestWriter.BaseStream.Position = posToFillEndOffest;
                            blockOffestWriter.Write(endOffest);
                            blockOffestWriter.BaseStream.Seek(0, SeekOrigin.End);
                            reader.ReadByte(); // 0x74
                            block.Add(s);
                        }
                        continue;
                }
                var exp = ParseCallExpressionWithoutType(reader, encoding, out string unexaminedCode, out string comment, out bool mask);
                switch (type)
                {
                    case 0x70: // 循环开始语句：XX循环首(参数...)
                        {
                            blockOffestWriter.Write((byte)3);
                            blockOffestWriter.Write(startOffest);
                            long posToFillEndOffest = blockOffestWriter.BaseStream.Position;
                            blockOffestWriter.Write(0);

                            var loopblock = ParseStatementBlock(reader, encoding, lineOffestWriter, blockOffestWriter);
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
                                    endexp = ParseCallExpressionWithoutType(reader, encoding, out endexp_unexaminedCode, out endexp_comment, out endexp_mask);
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
                    case 0x6C: // 如果真
                        {
                            blockOffestWriter.Write((byte)2);
                            blockOffestWriter.Write(startOffest);
                            long posToFillEndOffest = blockOffestWriter.BaseStream.Position;
                            blockOffestWriter.Write(0);

                            var s = new IfStatement()
                            {
                                Condition = exp.ParamList.ElementAtOrDefault(0),
                                UnexaminedCode = unexaminedCode,
                                Block = ParseStatementBlock(reader, encoding, lineOffestWriter, blockOffestWriter),
                                Comment = comment,
                                Mask = mask
                            };
                            if (reader.ReadByte() != 0x52)
                            {
                                throw new Exception();
                            }

                            var endOffest = (int)reader.BaseStream.Position;
                            blockOffestWriter.BaseStream.Position = posToFillEndOffest;
                            blockOffestWriter.Write(endOffest);
                            blockOffestWriter.BaseStream.Seek(0, SeekOrigin.End);

                            reader.ReadByte(); // 0x73

                            block.Add(s);
                        }
                        break;
                    case 0x6B: // 如果
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

                            s.BlockOnTrue = ParseStatementBlock(reader, encoding, lineOffestWriter, blockOffestWriter);
                            if (reader.ReadByte() != 0x50)
                            {
                                throw new Exception();
                            }
                            s.BlockOnFalse = ParseStatementBlock(reader, encoding, lineOffestWriter, blockOffestWriter);
                            if (reader.ReadByte() != 0x51)
                            {
                                throw new Exception();
                            }
                            var endOffest = (int)reader.BaseStream.Position;

                            blockOffestWriter.BaseStream.Position = posToFillEndOffest;
                            blockOffestWriter.Write(endOffest);
                            blockOffestWriter.BaseStream.Seek(0, SeekOrigin.End);

                            reader.ReadByte(); // 0x72

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
        private static Expression ParseExpression(BinaryReader reader, Encoding encoding, bool parseMember = true)
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
                        result = new StringLiteral(reader.ReadBStr(encoding));
                        break;
                    case 0x1B:
                        result = new ConstantExpression(reader.ReadInt32());
                        break;
                    case 0x1C:
                        result = new ConstantExpression((short)(reader.ReadInt16() - 1), (short)(reader.ReadInt16() - 1));
                        break;
                    case 0x1D:
                        // 0x1D 0x38 <Int32:VarId>
                        continue;
                    case 0x1E:
                        result = new MethodPtrExpression(reader.ReadInt32());
                        break;
                    case 0x21:
                        result = ParseCallExpressionWithoutType(reader, encoding);
                        break;
                    case 0x23:
                        result = new EmnuConstantExpression((short)(reader.ReadInt16() - 1), (short)(reader.ReadInt16() - 1), reader.ReadInt32() - 1);
                        break;
                    case 0x37:
                        continue;
                    case 0x1F:
                        {
                            var array = new ArrayLiteralExpression();
                            Expression exp;
                            while (!((exp = ParseExpression(reader, encoding)) is ArrayLiteralEnd))
                            {
                                array.Add(exp);
                            }
                            result = array;
                        }
                        break;
                    case 0x20:
                        result = ArrayLiteralEnd.Instance;
                        break;
                    case 0x38: // ThisCall Or 访问变量
                        {
                            int variable = reader.ReadInt32();
                            if (variable == 0x0500FFFE)
                            {
                                reader.ReadByte(); // 0x3A
                                return ParseExpression(reader, encoding, true);
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
                            int memberId = reader.ReadInt32();
                            if (EplSystemId.GetType(memberId) == EplSystemId.Type_StructMember)
                                result = new AccessMemberExpression(result, reader.ReadInt32(), memberId);
                            else
                                result = new AccessMemberExpression(result, (short)(reader.ReadInt16() - 1), (short)(reader.ReadInt16() - 1), memberId - 1);
                            break;
                        case 0x3A:
                            result = new AccessArrayExpression(result, ParseExpression(reader, encoding, false));
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

        private static ParamListExpression ParseParamList(BinaryReader reader, Encoding encoding)
        {
            var param = new ParamListExpression();
            Expression exp;
            while (!((exp = ParseExpression(reader, encoding)) is ParamListEnd))
            {
                param.Add(exp);
            }
            return param;
        }
        private static CallExpression ParseCallExpressionWithoutType(BinaryReader reader, Encoding encoding)
        {
            var exp = ParseCallExpressionWithoutType(reader, encoding, out string unexaminedCode, out string comment, out bool mask);
            return exp;
        }

        private static CallExpression ParseCallExpressionWithoutType(BinaryReader reader, Encoding encoding, out string unexaminedCode, out string comment, out bool mask)
        {
            var methodId = reader.ReadInt32();
            var libraryId = reader.ReadInt16();
            var flag = reader.ReadInt16();
            unexaminedCode = reader.ReadBStr(encoding);
            comment = reader.ReadBStr(encoding);
            mask = (flag & 0x20) != 0;
            bool invokeSpecial = (flag & 0x10) != 0;
            ////bool expand = (flag & 0x1) != 0;
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
            exp.InvokeSpecial = invokeSpecial;
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                switch (reader.ReadByte())
                {
                    case 0x36:
                        exp.ParamList = ParseParamList(reader, encoding);
                        break;
                    case 0x38: // ThisCall
                        reader.BaseStream.Position -= 1;
                        exp.Target = ParseExpression(reader, encoding);
                        exp.ParamList = ParseParamList(reader, encoding);
                        break;
                    default:
                        reader.BaseStream.Position -= 1;
                        throw new Exception();
                }
            }
            return exp;
        }
    }
}
