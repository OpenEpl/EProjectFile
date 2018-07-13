using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Linq;
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
        public int UnknownAfterId;
        public bool Public { get => (Flags & 0x8) != 0; set => Flags = (Flags & ~0x8) | (value ? 0x8 : 0); }
        public int Flags;
        public int BaseClass;
        public string Name;
        public string Comment;
        public int[] Method;
        public ClassVariableInfo[] Variables;

        public static ClassInfo[] ReadClasses(BinaryReader reader)
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
                    Name = reader.ReadStringWithLengthPrefix(),
                    Comment = reader.ReadStringWithLengthPrefix(),
                    Method = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4),
                    Variables = AbstractVariableInfo.ReadVariables(reader, x => new ClassVariableInfo(x))
                };
                classes[i] = classInfo;
            }

            return classes;
        }
        public static void WriteClasses(BinaryWriter writer, ClassInfo[] classes)
        {
            writer.Write(classes.Length * 8);
            Array.ForEach(classes, x => writer.Write(x.Id));
            Array.ForEach(classes, x => writer.Write(x.UnknownAfterId));
            foreach (var classInfo in classes)
            {
                writer.Write(classInfo.Flags);
                writer.Write(classInfo.BaseClass);
                writer.WriteStringWithLengthPrefix(classInfo.Name);
                writer.WriteStringWithLengthPrefix(classInfo.Comment);
                if (classInfo.Method == null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(classInfo.Method.Length * 4);
                    writer.WriteInt32sWithoutLengthPrefix(classInfo.Method);
                }
                AbstractVariableInfo.WriteVariables(writer, classInfo.Variables);
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        /// <summary>
        /// 到文本代码（结尾无换行）
        /// </summary>
        /// <param name="codeSection">若为null，不写出下属方法</param>
        /// <param name="nameMap">命名映射器</param>
        /// <param name="result">输出目标</param>
        /// <param name="indent">起始缩进</param>
        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent, CodeSectionInfo codeSection, bool writeCode = true)
        {
            TextCodeUtils.WriteDefinedCode(result, indent, "程序集", Name, BaseClass == 0 || BaseClass == -1 ? "" : nameMap.GetUserDefinedName(BaseClass), Public ? "公开" : "", Comment);
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
