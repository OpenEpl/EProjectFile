using Newtonsoft.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Sections
{
    public class CodeSection : IToTextCodeAble, ISection
    {
        private class KeyImpl : ISectionKey<CodeSection>
        {
            public string SectionName => "程序段";
            public int SectionKey => 0x03007319;
            public bool IsOptional => false;

            public CodeSection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                var codeSectionInfo = new CodeSection();
                int[] minRequiredCmds;
                short[] minRequiredDataTypes;
                short[] minRequiredConstants;
                using (var reader = new BinaryReader(new MemoryStream(data, false), encoding))
                {
                    codeSectionInfo.allocatedIdNum = reader.ReadInt32();
                    reader.ReadInt32(); // 确认于易语言V5.71
                    minRequiredCmds = reader.ReadInt32sWithByteSizePrefix();
                    if (cryptEC)
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
                        codeSectionInfo.UnknownBeforeIconData = reader.ReadBytes(16); // Unknown
                    }
                    codeSectionInfo.IconData = reader.ReadBytesWithLengthPrefix();
                    codeSectionInfo.DebugCommandParameters = reader.ReadStringWithLengthPrefix(encoding);
                    if (cryptEC)
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
                }
                return codeSectionInfo;
            }
        }

        public static readonly ISectionKey<CodeSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        private int allocatedIdNum;
        public LibraryRefInfo[] Libraries { get; set; }
        public int Flag { get; set; }
        /// <summary>
        /// 【仅用于不带有编辑信息的EC文件】“_启动子程序”，系统将在 初始模块段 保存的方法被调用完成后调用
        /// </summary>
        public int MainMethod { get; set; }
        [JsonIgnore]
        public byte[] UnknownBeforeIconData { get; set; }
        public byte[] IconData { get; set; }
        public string DebugCommandParameters { get; set; }
        public ClassInfo[] Classes { get; set; }
        public MethodInfo[] Methods { get; set; }
        public GlobalVariableInfo[] GlobalVariables { get; set; }
        public StructInfo[] Structs { get; set; }
        public DllDeclareInfo[] DllDeclares { get; set; }
        /// <summary>
        /// 分配一个Id
        /// </summary>
        /// <param name="type">参考EplSystemId.Type_***</param>
        /// <returns>指定类型的新Id</returns>
        public int AllocId(int type)
        {
            return ++allocatedIdNum | type;
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
            writer.Write(allocatedIdNum);
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
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            ToTextCode(nameMap, writer, indent, true);
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent, bool writeMethod, bool writeCode = true)
        {
            if (GlobalVariables != null && GlobalVariables.Length != 0)
            {
                TextCodeUtils.JoinAndWriteCode(GlobalVariables, Environment.NewLine, nameMap, writer, indent);
                writer.WriteLine();
            }
            TextCodeUtils.JoinAndWriteCode(Classes, Environment.NewLine, writeMethod ? this : null, nameMap, writer, indent, writeCode);
            if (DllDeclares != null && DllDeclares.Length != 0)
            {
                writer.WriteLine();
                TextCodeUtils.JoinAndWriteCode(DllDeclares, Environment.NewLine, nameMap, writer, indent);
            }
            if (Structs != null && Structs.Length != 0)
            {
                writer.WriteLine();
                TextCodeUtils.JoinAndWriteCode(Structs, Environment.NewLine, nameMap, writer, indent);
            }
        }
    }
}