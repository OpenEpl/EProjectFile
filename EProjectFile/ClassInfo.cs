using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Linq;
using QIQI.EProjectFile.Sections;
using QIQI.EProjectFile.Internal;
using System.Collections.Generic;

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
        public int Flags { get; set; }
        public int BaseClass { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public List<int> Method { get; set; }
        public List<ClassVariableInfo> Variables { get; set; }

        public static List<ClassInfo> ReadClasses(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndMemoryAddress((reader, id, memoryAddress) => new ClassInfo(id)
            {
                MemoryAddress = memoryAddress,
                Flags = reader.ReadInt32(),
                BaseClass = reader.ReadInt32(),
                Name = reader.ReadStringWithLengthPrefix(encoding),
                Comment = reader.ReadStringWithLengthPrefix(encoding),
                Method = reader.ReadInt32sListWithFixedLength(reader.ReadInt32() / 4),
                Variables = AbstractVariableInfo.ReadVariables(reader, encoding, x => new ClassVariableInfo(x))
            });
        }
        public static void WriteClasses(BinaryWriter w, Encoding encoding, List<ClassInfo> classes)
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
                    writer.Write(elem.Method.Count * 4);
                    foreach (var x in elem.Method) writer.Write(x);
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
        /// <param name="writer">输出目标</param>
        /// <param name="indent">起始缩进</param>
        /// <param name="codeSection">若为null，不写出下属方法</param>
        /// <param name="writeCode">是否输出子程序代码</param>
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent, CodeSection codeSection, bool writeCode = true)
        {
            TextCodeUtils.WriteDefinitionCode(writer, indent, "程序集", nameMap.GetUserDefinedName(Id), BaseClass == 0 || BaseClass == -1 ? "" : nameMap.GetUserDefinedName(BaseClass), "", Comment);
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
