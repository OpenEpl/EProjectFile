using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Linq;
using QIQI.EProjectFile.Sections;
using QIQI.EProjectFile.Internal;

namespace QIQI.EProjectFile
{
    public class ClassInfo : IHasId, IHasMemoryAddress, IToTextCodeAble
    {
        public int Id { get; }

        public ClassInfo(int id)
        {
            this.Id = id;
        }

        public int MemoryAddress { get; set; }
        public bool Public { get => (Flags & 0x8) != 0; set => Flags = (Flags & ~0x8) | (value ? 0x8 : 0); }
        public int Flags { get; set; }
        public int BaseClass { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public int[] Method { get; set; }
        public ClassVariableInfo[] Variables { get; set; }

        public static ClassInfo[] ReadClasses(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndMemoryAddress((reader, id, memoryAddress) => new ClassInfo(id)
            {
                MemoryAddress = memoryAddress,
                Flags = reader.ReadInt32(),
                BaseClass = reader.ReadInt32(),
                Name = reader.ReadStringWithLengthPrefix(encoding),
                Comment = reader.ReadStringWithLengthPrefix(encoding),
                Method = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4),
                Variables = AbstractVariableInfo.ReadVariables(reader, encoding, x => new ClassVariableInfo(x))
            });
        }
        public static void WriteClasses(BinaryWriter w, Encoding encoding, ClassInfo[] classes)
        {
            w.WriteBlocksWithIdAndMemoryAddress(classes, (writer, elem) =>
            {
                writer.Write(elem.Flags);
                writer.Write(elem.BaseClass);
                writer.WriteStringWithLengthPrefix(encoding, elem.Name);
                writer.WriteStringWithLengthPrefix(encoding, elem.Comment);
                if (elem.Method == null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(elem.Method.Length * 4);
                    writer.WriteInt32sWithoutLengthPrefix(elem.Method);
                }
                AbstractVariableInfo.WriteVariables(writer, encoding, elem.Variables);
            });
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        /// <summary>
        /// 到文本代码（结尾无换行）
        /// </summary>
        /// <param name="nameMap">命名映射器</param>
        /// <param name="result">输出目标</param>
        /// <param name="indent">起始缩进</param>
        /// <param name="codeSection">若为null，不写出下属方法</param>
        /// <param name="writeCode">是否输出子程序代码</param>
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent, CodeSection codeSection, bool writeCode = true)
        {
            TextCodeUtils.WriteDefinitionCode(writer, indent, "程序集", nameMap.GetUserDefinedName(Id), BaseClass == 0 || BaseClass == -1 ? "" : nameMap.GetUserDefinedName(BaseClass), Public ? "公开" : "", Comment);
            writer.WriteLine();
            TextCodeUtils.JoinAndWriteCode(Variables, Environment.NewLine, nameMap, writer, indent);
            if (codeSection != null) 
            {
                writer.WriteLine();
                writer.WriteLine();
                var methodId = Method.ToDictionary(x => x);
                TextCodeUtils.JoinAndWriteCode(codeSection.Methods.Where(x => methodId.ContainsKey(x.Id)), Environment.NewLine + Environment.NewLine, nameMap, writer, indent, writeCode);
            }
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent)
        {
            ToTextCode(nameMap, writer, indent, null);
        }
    }
}
