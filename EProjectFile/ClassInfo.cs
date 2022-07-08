using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Linq;
using QIQI.EProjectFile.Sections;

namespace QIQI.EProjectFile
{
    public class ClassInfo : IHasId, IToTextCodeAble
    {
        public int Id { get; }

        public ClassInfo(int id)
        {
            this.Id = id;
        }

        [JsonIgnore]
        public int UnknownAfterId { get; set; }
        public bool Public { get => (Flags & 0x8) != 0; set => Flags = (Flags & ~0x8) | (value ? 0x8 : 0); }
        public int Flags { get; set; }
        public int BaseClass { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public int[] Method { get; set; }
        public ClassVariableInfo[] Variables { get; set; }

        public static ClassInfo[] ReadClasses(BinaryReader reader, Encoding encoding)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var unknownsAfterIds = reader.ReadInt32sWithFixedLength(count);
            var classes = new ClassInfo[count];
            for (int i = 0; i < count; i++)
            {
                var classInfo = new ClassInfo(ids[i])
                {
                    UnknownAfterId = unknownsAfterIds[i],
                    Flags = reader.ReadInt32(),
                    BaseClass = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(encoding),
                    Comment = reader.ReadStringWithLengthPrefix(encoding),
                    Method = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4),
                    Variables = AbstractVariableInfo.ReadVariables(reader, encoding, x => new ClassVariableInfo(x))
                };
                classes[i] = classInfo;
            }

            return classes;
        }
        public static void WriteClasses(BinaryWriter writer, Encoding encoding, ClassInfo[] classes)
        {
            writer.Write(classes.Length * 8);
            Array.ForEach(classes, x => writer.Write(x.Id));
            Array.ForEach(classes, x => writer.Write(x.UnknownAfterId));
            foreach (var classInfo in classes)
            {
                writer.Write(classInfo.Flags);
                writer.Write(classInfo.BaseClass);
                writer.WriteStringWithLengthPrefix(encoding, classInfo.Name);
                writer.WriteStringWithLengthPrefix(encoding, classInfo.Comment);
                if (classInfo.Method == null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(classInfo.Method.Length * 4);
                    writer.WriteInt32sWithoutLengthPrefix(classInfo.Method);
                }
                AbstractVariableInfo.WriteVariables(writer, encoding, classInfo.Variables);
            }
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
        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent, CodeSection codeSection, bool writeCode = true)
        {
            TextCodeUtils.WriteDefinedCode(result, indent, "程序集", nameMap.GetUserDefinedName(Id), BaseClass == 0 || BaseClass == -1 ? "" : nameMap.GetUserDefinedName(BaseClass), Public ? "公开" : "", Comment);
            result.AppendLine();
            TextCodeUtils.WriteJoinCode(Variables, Environment.NewLine, nameMap, result, indent);
            if (codeSection != null) 
            {
                result.AppendLine();
                result.AppendLine();
                var methodId = Method.ToDictionary(x => x);
                TextCodeUtils.WriteJoinCode(codeSection.Methods.Where(x => methodId.ContainsKey(x.Id)), Environment.NewLine + Environment.NewLine, nameMap, result, indent, writeCode);
            }
        }
        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent)
        {
            ToTextCode(nameMap, result, indent, null);
        }
    }
}
