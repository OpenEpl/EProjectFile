using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class StructInfo :IHasId, IToTextCodeAble
    {
        public int Id { get; }

        public StructInfo(int id)
        {
            this.Id = id;
        }
        [JsonIgnore]
        public int UnknownAfterId;
        public int Flags;
        public bool Public { get => (Flags & 0x1) != 0; set => Flags = (Flags & ~0x1) | (value ? 0x1 : 0); }
        public string Name;
        public string Comment;
        public StructMemberInfo[] Member;
        public static StructInfo[] ReadStructs(BinaryReader reader)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var unknownsAfterIds = reader.ReadInt32sWithFixedLength(count);
            var structs = new StructInfo[count];
            for (int i = 0; i < count; i++)
            {
                var structInfo = new StructInfo(ids[i])
                {
                    UnknownAfterId = unknownsAfterIds[i],
                    Flags = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(),
                    Comment = reader.ReadStringWithLengthPrefix(),
                    Member = AbstractVariableInfo.ReadVariables(reader, x => new StructMemberInfo(x))
                };
                structs[i] = structInfo;
            }

            return structs;
        }
        public static void WriteStructs(BinaryWriter writer, StructInfo[] structs)
        {
            writer.Write(structs.Length * 8);
            Array.ForEach(structs, x => writer.Write(x.Id));
            Array.ForEach(structs, x => writer.Write(x.UnknownAfterId));
            foreach (var structInfo in structs)
            {
                writer.Write(structInfo.Flags);
                writer.WriteStringWithLengthPrefix(structInfo.Name);
                writer.WriteStringWithLengthPrefix(structInfo.Comment);
                AbstractVariableInfo.WriteVariables(writer, structInfo.Member);
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent)
        {
            TextCodeUtils.WriteDefinedCode(result, indent, "数据类型", nameMap.GetUserDefinedName(Id), Public ? "公开" : "", Comment);
            result.AppendLine();
            TextCodeUtils.WriteJoinCode(Member, Environment.NewLine, nameMap, result, indent + 1);
        }
    }
}
