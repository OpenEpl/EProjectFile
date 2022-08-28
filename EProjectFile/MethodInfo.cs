using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile
{
    public struct MethodCodeData
    {
        [JsonInclude]
        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] LineOffest;
        [JsonInclude]
        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] BlockOffest;
        [JsonInclude]
        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] MethodReference;
        [JsonInclude]
        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] VariableReference;
        [JsonInclude]
        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] ConstantReference;
        [JsonInclude]
        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] ExpressionData;
        [JsonInclude]
        [JsonConverter(typeof(EncodingJsonConverter))]
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
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
    public class MethodInfo : IHasId, IHasMemoryAddress, IToTextCodeAble
    {
        public int Id { get; }

        public MethodInfo(int id)
        {
            this.Id = id;
        }

        public int MemoryAddress { get; set; }
        /// <summary>
        /// 所属程序集Id
        /// </summary>
        public int Class { get; set; }
        public int Flags { get; set; }
        public bool Public { get => (Flags & 0x8) != 0; set => Flags = (Flags & ~0x8) | (value ? 0x8 : 0); }
        public bool Hidden { get => (Flags & 0x80) != 0; set => Flags = (Flags & ~0x80) | (value ? 0x80 : 0); }
        public int ReturnDataType { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public List<LocalVariableInfo> Variables { get; set; }
        public List<MethodParameterInfo> Parameters { get; set; }
        public MethodCodeData CodeData { get; set; }
        public bool IsStatic => EplSystemId.GetType(Class) == EplSystemId.Type_StaticClass || EplSystemId.GetType(Class) == EplSystemId.Type_FormClass;
        public static List<MethodInfo> ReadMethods(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndMemoryAddress((reader, id, memoryAddress) =>
            {
                var elem = new MethodInfo(id)
                {
                    MemoryAddress = memoryAddress,
                    Class = reader.ReadInt32(),
                    Flags = reader.ReadInt32(),
                    ReturnDataType = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(encoding),
                    Comment = reader.ReadStringWithLengthPrefix(encoding),
                    Variables = AbstractVariableInfo.ReadVariables(reader, encoding, x => new LocalVariableInfo(x)),
                    Parameters = AbstractVariableInfo.ReadVariables(reader, encoding, x => new MethodParameterInfo(x))
                };
                elem.CodeData = new MethodCodeData(
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    reader.ReadBytesWithLengthPrefix(),
                    encoding);
                return elem;
            });
        }
        public static void WriteMethods(BinaryWriter w, Encoding encoding, List<MethodInfo> methods)
        {
            w.WriteBlocksWithIdAndMemoryAddress(methods, (writer, elem) =>
            {
                writer.Write(elem.Class);
                writer.Write(elem.Flags);
                writer.Write(elem.ReturnDataType);
                writer.WriteStringWithLengthPrefix(encoding, elem.Name);
                writer.WriteStringWithLengthPrefix(encoding, elem.Comment);
                AbstractVariableInfo.WriteVariables(writer, encoding, elem.Variables);
                AbstractVariableInfo.WriteVariables(writer, encoding, elem.Parameters);
                writer.WriteBytesWithLengthPrefix(elem.CodeData.LineOffest);
                writer.WriteBytesWithLengthPrefix(elem.CodeData.BlockOffest);
                writer.WriteBytesWithLengthPrefix(elem.CodeData.MethodReference);
                writer.WriteBytesWithLengthPrefix(elem.CodeData.VariableReference);
                writer.WriteBytesWithLengthPrefix(elem.CodeData.ConstantReference);
                writer.WriteBytesWithLengthPrefix(elem.CodeData.ExpressionData);
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent, bool writeCode)
        {
            TextCodeUtils.WriteDefinitionCode(writer, indent, "子程序", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(ReturnDataType), Public ? "公开" : "", Comment);
            if (Parameters != null && Parameters.Count != 0)
            {
                writer.WriteLine();
                TextCodeUtils.JoinAndWriteCode(Parameters, Environment.NewLine, nameMap, writer, indent);
            }
            if (!writeCode) return;
            if (Variables != null && Variables.Count != 0)
            {
                writer.WriteLine();
                TextCodeUtils.JoinAndWriteCode(Variables, Environment.NewLine, nameMap, writer, indent);
            }
            writer.WriteLine();
            writer.WriteLine();
            CodeDataParser.ParseStatementBlock(CodeData.ExpressionData, CodeData.Encoding).ToTextCode(nameMap, writer, indent);
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            ToTextCode(nameMap, writer, indent, true);
        }
    }
}
