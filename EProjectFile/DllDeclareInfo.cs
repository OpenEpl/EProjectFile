using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class DllDeclareInfo : IHasId, IToTextCodeAble
    {
        public int Id { get; }

        public DllDeclareInfo(int id)
        {
            this.Id = id;
        }

        [JsonIgnore]
        public int UnknownAfterId { get; set; }
        public int Flags { get; set; }
        public bool Public { get => (Flags & 0x2) != 0; set => Flags = (Flags & ~0x2) | (value ? 0x2 : 0); }
        public int ReturnDataType { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string EntryPoint { get; set; }
        public string LibraryName { get; set; }
        public DllParameterInfo[] Parameters { get; set; }
        public static DllDeclareInfo[] ReadDllDeclares(BinaryReader reader)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var unknownsAfterIds = reader.ReadInt32sWithFixedLength(count);
            var dllDeclares = new DllDeclareInfo[count];
            for (int i = 0; i < count; i++)
            {
                var dllDeclareInfo = new DllDeclareInfo(ids[i])
                {
                    UnknownAfterId = unknownsAfterIds[i],
                    Flags = reader.ReadInt32(),
                    ReturnDataType = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(),
                    Comment = reader.ReadStringWithLengthPrefix(),
                    LibraryName = reader.ReadStringWithLengthPrefix(),
                    EntryPoint = reader.ReadStringWithLengthPrefix(),
                    Parameters = AbstractVariableInfo.ReadVariables(reader, x => new DllParameterInfo(x))
                };
                dllDeclares[i] = dllDeclareInfo;
            }

            return dllDeclares;
        }
        public static void WriteDllDeclares(BinaryWriter writer, DllDeclareInfo[] dllDeclares)
        {
            writer.Write(dllDeclares.Length * 8);
            Array.ForEach(dllDeclares, x => writer.Write(x.Id));
            Array.ForEach(dllDeclares, x => writer.Write(x.UnknownAfterId));
            foreach (var dllDeclare in dllDeclares)
            {
                writer.Write(dllDeclare.Flags);
                writer.Write(dllDeclare.ReturnDataType);
                writer.WriteStringWithLengthPrefix(dllDeclare.Name);
                writer.WriteStringWithLengthPrefix(dllDeclare.Comment);
                writer.WriteStringWithLengthPrefix(dllDeclare.LibraryName);
                writer.WriteStringWithLengthPrefix(dllDeclare.EntryPoint);
                AbstractVariableInfo.WriteVariables(writer, dllDeclare.Parameters);
            }
        }
        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent)
        {
            TextCodeUtils.WriteDefinedCode(result, indent, "DLL命令", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(ReturnDataType), LibraryName, EntryPoint, Public ? "公开" : "", Comment);
            result.AppendLine();
            TextCodeUtils.WriteJoinCode(Parameters, Environment.NewLine, nameMap, result, indent + 1);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
