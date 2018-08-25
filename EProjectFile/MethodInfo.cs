using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public struct MethodCodeData
    {
        [JsonConverter(typeof(HexConverter))]
        public byte[] LineOffest;
        [JsonConverter(typeof(HexConverter))]
        public byte[] BlockOffest;
        [JsonConverter(typeof(HexConverter))]
        public byte[] MethodReference;
        [JsonConverter(typeof(HexConverter))]
        public byte[] VariableReference;
        [JsonConverter(typeof(HexConverter))]
        public byte[] ConstantReference;
        [JsonConverter(typeof(HexConverter))]
        public byte[] ExpressionData;
        [JsonIgnore]
        public Encoding Encoding;
        [Obsolete]
        public MethodCodeData(byte[] lineOffest, byte[] blockOffest, byte[] methodReference, byte[] variableReference, byte[] constantReference, byte[] expressionData)
            : this(lineOffest, blockOffest, methodReference, variableReference, constantReference, expressionData, Encoding.GetEncoding("gbk"))
        {
            // Nothing need doing.
        }
        public MethodCodeData(byte[] lineOffest, byte[] blockOffest, byte[] methodReference, byte[] variableReference, byte[] constantReference, byte[] expressionData, Encoding encoding)
        {
            LineOffest = lineOffest;
            BlockOffest = blockOffest;
            MethodReference = methodReference;
            VariableReference = variableReference;
            ConstantReference = constantReference;
            ExpressionData = expressionData;
            Encoding = encoding;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class MethodInfo : IHasId, IToTextCodeAble
    {
        public int Id { get; }

        public MethodInfo(int id)
        {
            this.Id = id;
        }
        [JsonIgnore]
        public int UnknownAfterId { get; set; }
        /// <summary>
        /// 所属程序集Id
        /// </summary>
        public int Class { get; set; }
        public int Flags { get; set; }
        public bool Public { get => (Flags & 0x8) != 0; set => Flags = (Flags & ~0x8) | (value ? 0x8 : 0); }
        public int ReturnDataType { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public LocalVariableInfo[] Variables { get; set; }
        public MethodParameterInfo[] Parameters { get; set; }
        public MethodCodeData CodeData { get; set; }
        public static MethodInfo[] ReadMethods(BinaryReader reader, Encoding encoding)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var unknownsAfterIds = reader.ReadInt32sWithFixedLength(count);
            var methods = new MethodInfo[count];
            for (int i = 0; i < count; i++)
            {
                var methodInfo = new MethodInfo(ids[i])
                {
                    UnknownAfterId = unknownsAfterIds[i],
                    Class = reader.ReadInt32(),
                    Flags = reader.ReadInt32(),
                    ReturnDataType = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(encoding),
                    Comment = reader.ReadStringWithLengthPrefix(encoding),
                    Variables = AbstractVariableInfo.ReadVariables(reader, encoding, x => new LocalVariableInfo(x)),
                    Parameters = AbstractVariableInfo.ReadVariables(reader, encoding, x => new MethodParameterInfo(x))
                };
                methodInfo.CodeData = new MethodCodeData(
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    encoding);
                methods[i] = methodInfo;
            }

            return methods;
        }
        public static void WriteMethods(BinaryWriter writer, Encoding encoding, MethodInfo[] methods)
        {
            writer.Write(methods.Length * 8);
            Array.ForEach(methods, x => writer.Write(x.Id));
            Array.ForEach(methods, x => writer.Write(x.UnknownAfterId));
            foreach (var method in methods)
            {
                writer.Write(method.Class);
                writer.Write(method.Flags);
                writer.Write(method.ReturnDataType);
                writer.WriteStringWithLengthPrefix(encoding, method.Name);
                writer.WriteStringWithLengthPrefix(encoding, method.Comment);
                AbstractVariableInfo.WriteVariables(writer, encoding, method.Variables);
                AbstractVariableInfo.WriteVariables(writer, encoding, method.Parameters);
                writer.WriteBytesWithLengthPrefix(method.CodeData.LineOffest);
                writer.WriteBytesWithLengthPrefix(method.CodeData.BlockOffest);
                writer.WriteBytesWithLengthPrefix(method.CodeData.MethodReference);
                writer.WriteBytesWithLengthPrefix(method.CodeData.VariableReference);
                writer.WriteBytesWithLengthPrefix(method.CodeData.ConstantReference);
                writer.WriteBytesWithLengthPrefix(method.CodeData.ExpressionData);
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent, bool writeCode)
        {
            TextCodeUtils.WriteDefinedCode(result, indent, "子程序", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(ReturnDataType), Public ? "公开" : "", Comment);
            if (Parameters != null && Parameters.Length != 0)
            {
                result.AppendLine();
                TextCodeUtils.WriteJoinCode(Parameters, Environment.NewLine, nameMap, result, indent);
            }
            if (!writeCode) return;
            if (Variables != null && Variables.Length != 0)
            {
                result.AppendLine();
                TextCodeUtils.WriteJoinCode(Variables, Environment.NewLine, nameMap, result, indent);
            }
            result.AppendLine();
            result.AppendLine();
            CodeDataParser.ParseStatementBlock(CodeData.ExpressionData, CodeData.Encoding).ToTextCode(nameMap, result, indent);
        }
        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            ToTextCode(nameMap, result, indent, true);
        }
    }
}
