﻿using System.Text.Json;
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
        /// <summary>
        /// 窗口程序集 中指定关联窗口的 <see cref="FormInfo.Id"/>
        /// </summary>
        /// <seealso cref="FormInfo.Class"/>
        public int Form { get; set; }
        public int BaseClass { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public List<int> Methods { get; set; }
        public List<ClassVariableInfo> Variables { get; set; }

        public static List<ClassInfo> ReadClasses(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndMemoryAddress((reader, id, memoryAddress) => new ClassInfo(id)
            {
                MemoryAddress = memoryAddress,
                Form = reader.ReadInt32(),
                BaseClass = reader.ReadInt32(),
                Name = reader.ReadStringWithLengthPrefix(encoding),
                Comment = reader.ReadStringWithLengthPrefix(encoding),
                Methods = reader.ReadInt32sListWithFixedLength(reader.ReadInt32() / 4),
                Variables = AbstractVariableInfo.ReadVariables(reader, encoding, x => new ClassVariableInfo(x))
            });
        }
        public static void WriteClasses(BinaryWriter w, Encoding encoding, List<ClassInfo> classes)
        {
            w.WriteBlocksWithIdAndMemoryAddress(classes, (writer, elem) =>
            {
                writer.Write(elem.Form);
                writer.Write(elem.BaseClass);
                writer.WriteStringWithLengthPrefix(encoding, elem.Name);
                writer.WriteStringWithLengthPrefix(encoding, elem.Comment);
                if (elem.Methods == null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(elem.Methods.Count * 4);
                    foreach (var x in elem.Methods) writer.Write(x);
                }
                AbstractVariableInfo.WriteVariables(writer, encoding, elem.Variables);
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
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
                var methodId = Methods.ToDictionary(x => x);
                TextCodeUtils.JoinAndWriteCode(codeSection.Methods.Where(x => methodId.ContainsKey(x.Id)), Environment.NewLine + Environment.NewLine, nameMap, writer, indent, writeCode);
            }
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent)
        {
            ToTextCode(nameMap, writer, indent, null);
        }
    }
}
