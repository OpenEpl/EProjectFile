using Newtonsoft.Json;
using System.IO;

namespace QIQI.EProjectFile
{
    public class CodeSectionInfo
    {
        public const string SectionName = "程序段";

        private int AllocatedIdNum;
        [JsonIgnore]
        public byte[] UnknownBeforeLibrary_1;
        [JsonIgnore]
        public byte[] UnknownBeforeLibrary_2;
        [JsonIgnore]
        public byte[] UnknownBeforeLibrary_3;
        public LibraryInfo[] Libraries;
        public int Flag;
        /// <summary>
        /// 【仅用于不带有编辑信息的EC文件】“_启动子程序”，系统将在 初始模块段 保存的方法被调用完成后调用
        /// </summary>
        public int MainMethod;
        [JsonIgnore]
        public byte[] UnknownBeforeIconData;
        public byte[] IconData;
        public string DebugCommandParameters;
        public ClassInfo[] Classes;
        public MethodInfo[] Methods;
        public VariableInfo[] GlobalVariables;
        public StructInfo[] Structs;
        public DllDeclareInfo[] DllDeclares;

        public static CodeSectionInfo Parse(byte[] data, bool cryptEc = false)
        {
            var codeSectionInfo = new CodeSectionInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false)))
            {
                codeSectionInfo.AllocatedIdNum = reader.ReadInt32();
                reader.ReadInt32();//确认于易语言V5.71
                codeSectionInfo.UnknownBeforeLibrary_1 = reader.ReadBytesWithLengthPrefix();//Unknown
                if (cryptEc)
                {
                    reader.ReadInt32();
                    reader.ReadInt32();
                    codeSectionInfo.UnknownBeforeLibrary_2 = reader.ReadBytesWithLengthPrefix();//Unknown
                    codeSectionInfo.Flag = reader.ReadInt32();
                    codeSectionInfo.MainMethod = reader.ReadInt32();
                    codeSectionInfo.Libraries = LibraryInfo.ReadLibraries(reader);
                    codeSectionInfo.UnknownBeforeLibrary_3 = reader.ReadBytesWithLengthPrefix();//Unknown
                }
                else
                {
                    codeSectionInfo.UnknownBeforeLibrary_2 = reader.ReadBytesWithLengthPrefix();//Unknown
                    codeSectionInfo.UnknownBeforeLibrary_3 = reader.ReadBytesWithLengthPrefix();//Unknown
                    codeSectionInfo.Libraries = LibraryInfo.ReadLibraries(reader);
                    codeSectionInfo.Flag = reader.ReadInt32();
                    codeSectionInfo.MainMethod = reader.ReadInt32();
                }
;
                if ((codeSectionInfo.Flag & 1) != 0)
                {
                    codeSectionInfo.UnknownBeforeIconData = reader.ReadBytes(16);//Unknown
                }
                codeSectionInfo.IconData = reader.ReadBytesWithLengthPrefix();
                codeSectionInfo.DebugCommandParameters = reader.ReadStringWithLengthPrefix();
                if (cryptEc)
                {
                    reader.ReadBytes(12);
                    codeSectionInfo.Methods = MethodInfo.ReadMethods(reader);
                    codeSectionInfo.DllDeclares = DllDeclareInfo.ReadDllDeclares(reader);
                    codeSectionInfo.GlobalVariables = VariableInfo.ReadVariables(reader);
                    codeSectionInfo.Classes = ClassInfo.ReadClasses(reader);
                    codeSectionInfo.Structs = StructInfo.ReadStructs(reader);
                }
                else
                {
                    codeSectionInfo.Classes = ClassInfo.ReadClasses(reader);
                    codeSectionInfo.Methods = MethodInfo.ReadMethods(reader);
                    codeSectionInfo.GlobalVariables = VariableInfo.ReadVariables(reader);
                    codeSectionInfo.Structs = StructInfo.ReadStructs(reader);
                    codeSectionInfo.DllDeclares = DllDeclareInfo.ReadDllDeclares(reader);
                }
            }
            return codeSectionInfo;
        }
        /// <summary>
        /// 分配一个Id
        /// </summary>
        /// <param name="type">参考EplSystemId.Type_***</param>
        /// <returns></returns>
        public int AllocId(int type)
        {
            return ++AllocatedIdNum | type;
        }
        public byte[] ToBytes()
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                WriteTo(writer);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }
        private void WriteTo(BinaryWriter writer)
        {
            writer.Write(AllocatedIdNum);
            writer.Write(51113791);//确认于易语言V5.71
            writer.WriteBytesWithLengthPrefix(UnknownBeforeLibrary_1);
            writer.WriteBytesWithLengthPrefix(UnknownBeforeLibrary_2);
            writer.WriteBytesWithLengthPrefix(UnknownBeforeLibrary_3);
            LibraryInfo.WriteLibraries(writer, Libraries);
            writer.Write(Flag);
            writer.Write(MainMethod);
            if (UnknownBeforeIconData != null)
            {
                writer.WriteBytesWithLengthPrefix(UnknownBeforeIconData);
            }
            writer.WriteBytesWithLengthPrefix(IconData);
            writer.WriteStringWithLengthPrefix(DebugCommandParameters);
            ClassInfo.WriteClasses(writer, Classes);
            MethodInfo.WriteMethods(writer, Methods);
            VariableInfo.WriteVariables(writer, GlobalVariables);
            StructInfo.WriteStructs(writer, Structs);
            DllDeclareInfo.WriteDllDeclares(writer, DllDeclares);
            writer.Write(new byte[40]);//Unknown（40个0）
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}