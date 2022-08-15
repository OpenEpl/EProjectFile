using Newtonsoft.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class DllDeclareInfo : IHasId, IHasMemoryAddress, IToTextCodeAble
    {
        public int Id { get; }

        public DllDeclareInfo(int id)
        {
            this.Id = id;
        }

        public int MemoryAddress { get; set; }
        public int Flags { get; set; }
        public bool Public { get => (Flags & 0x2) != 0; set => Flags = (Flags & ~0x2) | (value ? 0x2 : 0); }
        public bool Hidden { get => (Flags & 0x4) != 0; set => Flags = (Flags & ~0x4) | (value ? 0x4 : 0); }
        public int ReturnDataType { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string EntryPoint { get; set; }
        public string LibraryName { get; set; }
        public List<DllParameterInfo> Parameters { get; set; }
        public static List<DllDeclareInfo> ReadDllDeclares(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndMemoryAddress((reader, id, memoryAddress) => new DllDeclareInfo(id)
            {
                MemoryAddress = memoryAddress,
                Flags = reader.ReadInt32(),
                ReturnDataType = reader.ReadInt32(),
                Name = reader.ReadStringWithLengthPrefix(encoding),
                Comment = reader.ReadStringWithLengthPrefix(encoding),
                LibraryName = reader.ReadStringWithLengthPrefix(encoding),
                EntryPoint = reader.ReadStringWithLengthPrefix(encoding),
                Parameters = AbstractVariableInfo.ReadVariables(reader, encoding, x => new DllParameterInfo(x))
            });
        }
        public static void WriteDllDeclares(BinaryWriter w, Encoding encoding, List<DllDeclareInfo> dllDeclares)
        {
            w.WriteBlocksWithIdAndMemoryAddress(dllDeclares, (writer, elem) =>
            {
                writer.Write(elem.Flags);
                writer.Write(elem.ReturnDataType);
                writer.WriteStringWithLengthPrefix(encoding, elem.Name);
                writer.WriteStringWithLengthPrefix(encoding, elem.Comment);
                writer.WriteStringWithLengthPrefix(encoding, elem.LibraryName);
                writer.WriteStringWithLengthPrefix(encoding, elem.EntryPoint);
                AbstractVariableInfo.WriteVariables(writer, encoding, elem.Parameters);
            });
        }
        public void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent)
        {
            TextCodeUtils.WriteDefinitionCode(writer, indent, "DLL命令", nameMap.GetUserDefinedName(Id), nameMap.GetDataTypeName(ReturnDataType), LibraryName, EntryPoint, Public ? "公开" : "", Comment);
            writer.WriteLine();
            TextCodeUtils.JoinAndWriteCode(Parameters, Environment.NewLine, nameMap, writer, indent + 1);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
