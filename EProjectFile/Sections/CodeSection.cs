using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class CodeSection : IToTextCodeAble, ISection
    {
        private class KeyImpl : ISectionKey<CodeSection>
        {
            public string SectionName => "程序段";
            public int SectionKey => 0x03007319;
            public bool IsOptional => false;

            public CodeSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    var codeSectionInfo = new CodeSection();
                    int[] minRequiredCmds;
                    short[] minRequiredDataTypes;
                    short[] minRequiredConstants;
                    codeSectionInfo.AllocatedIdNum = reader.ReadInt32();
                    reader.ReadInt32(); // 确认于易语言V5.71
                    minRequiredCmds = reader.ReadInt32sWithByteSizePrefix();
                    if (context.CryptEC)
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        minRequiredDataTypes = reader.ReadInt16sWithByteSizePrefix();
                        codeSectionInfo.Flag = reader.ReadInt32();
                        codeSectionInfo.MainMethod = reader.ReadInt32();
                        codeSectionInfo.Libraries = LibraryRefInfo.ReadLibraries(reader, encoding);
                        minRequiredConstants = reader.ReadInt16sWithByteSizePrefix();
                    }
                    else
                    {
                        minRequiredDataTypes = reader.ReadInt16sWithByteSizePrefix();
                        minRequiredConstants = reader.ReadInt16sWithByteSizePrefix();
                        codeSectionInfo.Libraries = LibraryRefInfo.ReadLibraries(reader, encoding);
                        codeSectionInfo.Flag = reader.ReadInt32();
                        codeSectionInfo.MainMethod = reader.ReadInt32();
                    }
                    LibraryRefInfo.ApplyCompatibilityInfo(codeSectionInfo.Libraries, minRequiredCmds, minRequiredDataTypes, minRequiredConstants);
                    if ((codeSectionInfo.Flag & 1) != 0)
                    {
                        codeSectionInfo.UnknownBeforeIconData = reader.ReadImmutableBytes(16); // Unknown
                    }
                    codeSectionInfo.IconData = reader.ReadBytesWithLengthPrefix();
                    codeSectionInfo.DebugCommandParameters = reader.ReadStringWithLengthPrefix(encoding);
                    if (context.CryptEC)
                    {
                        reader.ReadBytes(12);
                        codeSectionInfo.Methods = MethodInfo.ReadMethods(reader, encoding);
                        codeSectionInfo.DllDeclares = DllDeclareInfo.ReadDllDeclares(reader, encoding);
                        codeSectionInfo.GlobalVariables = AbstractVariableInfo.ReadVariables(reader, encoding, x => new GlobalVariableInfo(x));
                        codeSectionInfo.Classes = ClassInfo.ReadClasses(reader, encoding);
                        codeSectionInfo.Structs = StructInfo.ReadStructs(reader, encoding);
                    }
                    else
                    {
                        codeSectionInfo.Classes = ClassInfo.ReadClasses(reader, encoding);
                        codeSectionInfo.Methods = MethodInfo.ReadMethods(reader, encoding);
                        codeSectionInfo.GlobalVariables = AbstractVariableInfo.ReadVariables(reader, encoding, x => new GlobalVariableInfo(x));
                        codeSectionInfo.Structs = StructInfo.ReadStructs(reader, encoding);
                        codeSectionInfo.DllDeclares = DllDeclareInfo.ReadDllDeclares(reader, encoding);
                    }
                    return codeSectionInfo;
                });
            }
        }

        public static readonly ISectionKey<CodeSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        /// <summary>
        /// 已分配的Id的数值部分的最大值，分配新Id时，其 <c>IdNum = <see cref="AllocatedIdNum"/> + 1</c>。<br/>
        /// 在不分配任何Id的情况下，默认为 <c>0xFFFF</c>（与易语言内部保持一致），即通常第一个有效Id为 <c>0x10000</c>。
        /// </summary>
        /// <remarks>
        /// 该值允许外部修改，但应仅用于特殊用途；对于常规的分配Id操作，请使用 <see cref="AllocId(int)"/>。
        /// </remarks>
        /// <seealso cref="AllocId(int)"/>
        [DefaultValue(0xFFFF)]
        public int AllocatedIdNum { get; set; } = 0xFFFF;
        public LibraryRefInfo[] Libraries { get; set; }
        public int Flag { get; set; }
        /// <summary>
        /// 【仅用于不带有编辑信息的EC文件】“_启动子程序”，系统将在 初始模块段 保存的方法被调用完成后调用
        /// </summary>
        public int MainMethod { get; set; }
        [JsonIgnore]
        public ImmutableArray<byte> UnknownBeforeIconData { get; set; }
        public byte[] IconData { get; set; }
        public string DebugCommandParameters { get; set; }
        public List<ClassInfo> Classes { get; set; }
        public List<MethodInfo> Methods { get; set; }
        public List<GlobalVariableInfo> GlobalVariables { get; set; }
        public List<StructInfo> Structs { get; set; }
        public List<DllDeclareInfo> DllDeclares { get; set; }
        /// <summary>
        /// 分配一个Id，并自动更新 <see cref="AllocatedIdNum"/>
        /// </summary>
        /// <param name="type">可参考 <see cref="EplSystemId"/> 中以 <c>Type_</c> 开头的常量</param>
        /// <returns>指定类型的新Id</returns>
        /// <seealso cref="AllocatedIdNum"/>
        public int AllocId(int type)
        {
            return ++AllocatedIdNum | type;
        }
        public byte[] ToBytes(Encoding encoding)
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream(), encoding))
            {
                WriteTo(writer, encoding);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }
        private void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            writer.Write(AllocatedIdNum);
            writer.Write(51113791); // 确认于易语言V5.71
            LibraryRefInfo.WriteLibraries(writer, encoding, Libraries);
            writer.Write(Flag);
            writer.Write(MainMethod);
            if (UnknownBeforeIconData != null)
            {
                writer.Write(UnknownBeforeIconData);
            }
            writer.WriteBytesWithLengthPrefix(IconData);
            writer.WriteStringWithLengthPrefix(encoding, DebugCommandParameters);
            ClassInfo.WriteClasses(writer, encoding, Classes);
            MethodInfo.WriteMethods(writer, encoding, Methods);
            AbstractVariableInfo.WriteVariables(writer, encoding, GlobalVariables);
            StructInfo.WriteStructs(writer, encoding, Structs);
            DllDeclareInfo.WriteDllDeclares(writer, encoding, DllDeclares);
            writer.Write(new byte[40]); // Unknown（40个0）
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            ToTextCode(nameMap, writer, indent, true);
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent, bool writeMethod, bool writeCode = true)
        {
            if (GlobalVariables != null && GlobalVariables.Count != 0)
            {
                TextCodeUtils.JoinAndWriteCode(GlobalVariables, Environment.NewLine, nameMap, writer, indent);
                writer.WriteLine();
            }
            TextCodeUtils.JoinAndWriteCode(Classes, Environment.NewLine, writeMethod ? this : null, nameMap, writer, indent, writeCode);
            if (DllDeclares != null && DllDeclares.Count != 0)
            {
                writer.WriteLine();
                TextCodeUtils.JoinAndWriteCode(DllDeclares, Environment.NewLine, nameMap, writer, indent);
            }
            if (Structs != null && Structs.Count != 0)
            {
                writer.WriteLine();
                TextCodeUtils.JoinAndWriteCode(Structs, Environment.NewLine, nameMap, writer, indent);
            }
        }
    }
}