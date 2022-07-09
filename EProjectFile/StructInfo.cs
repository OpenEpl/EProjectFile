using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class StructInfo : IHasId, IToTextCodeAble
    {
        public int Id { get; }

        public StructInfo(int id)
        {
            this.Id = id;
        }
        [JsonIgnore]
        public int MemoryAddress { get; set; }
        public int Flags { get; set; }
        public bool Public { get => (Flags & 0x1) != 0; set => Flags = (Flags & ~0x1) | (value ? 0x1 : 0); }
        public string Name { get; set; }
        public string Comment { get; set; }
        public StructMemberInfo[] Member { get; set; }
        public static StructInfo[] ReadStructs(BinaryReader reader, Encoding encoding)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var memoryAddresses = reader.ReadInt32sWithFixedLength(count);
            var structs = new StructInfo[count];
            for (int i = 0; i < count; i++)
            {
                var structInfo = new StructInfo(ids[i])
                {
                    MemoryAddress = memoryAddresses[i],
                    Flags = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(encoding),
                    Comment = reader.ReadStringWithLengthPrefix(encoding),
                    Member = AbstractVariableInfo.ReadVariables(reader, encoding, x => new StructMemberInfo(x))
                };
                structs[i] = structInfo;
            }

            return structs;
        }
        public static void WriteStructs(BinaryWriter writer, Encoding encoding, StructInfo[] structs)
        {
            writer.Write(structs.Length * 8);
            Array.ForEach(structs, x => writer.Write(x.Id));
            Array.ForEach(structs, x => writer.Write(x.MemoryAddress));
            foreach (var structInfo in structs)
            {
                writer.Write(structInfo.Flags);
                writer.WriteStringWithLengthPrefix(encoding, structInfo.Name);
                writer.WriteStringWithLengthPrefix(encoding, structInfo.Comment);
                AbstractVariableInfo.WriteVariables(writer, encoding, structInfo.Member);
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
