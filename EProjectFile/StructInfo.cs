using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class StructInfo : IHasId, IHasMemoryAddress, IToTextCodeAble
    {
        public int Id { get; }

        public StructInfo(int id)
        {
            this.Id = id;
        }

        public int MemoryAddress { get; set; }
        public int Flags { get; set; }
        public bool Public { get => (Flags & 0x1) != 0; set => Flags = (Flags & ~0x1) | (value ? 0x1 : 0); }
        public string Name { get; set; }
        public string Comment { get; set; }
        public StructMemberInfo[] Member { get; set; }
        public static StructInfo[] ReadStructs(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndMemoryAddress((reader, id, memoryAddress) =>
                new StructInfo(id)
                {
                    MemoryAddress = memoryAddress,
                    Flags = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(encoding),
                    Comment = reader.ReadStringWithLengthPrefix(encoding),
                    Member = AbstractVariableInfo.ReadVariables(reader, encoding, x => new StructMemberInfo(x))
                }
            );
        }
        public static void WriteStructs(BinaryWriter w, Encoding encoding, StructInfo[] structs)
        {
            w.WriteBlocksWithIdAndMemoryAddress(structs, (writer, elem) =>
            {
                writer.Write(elem.Flags);
                writer.WriteStringWithLengthPrefix(encoding, elem.Name);
                writer.WriteStringWithLengthPrefix(encoding, elem.Comment);
                AbstractVariableInfo.WriteVariables(writer, encoding, elem.Member);
            });
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent)
        {
            TextCodeUtils.WriteDefinitionCode(writer, indent, "数据类型", nameMap.GetUserDefinedName(Id), Public ? "公开" : "", Comment);
            writer.WriteLine();
            TextCodeUtils.JoinAndWriteCode(Member, Environment.NewLine, nameMap, writer, indent + 1);
        }
    }
}
