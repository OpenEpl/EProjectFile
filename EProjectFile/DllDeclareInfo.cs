using Newtonsoft.Json;
using System;
using System.IO;

namespace QIQI.EProjectFile
{
    public class DllDeclareInfo :IHasId
    {
        private int id;
        public int Id => id;
        public DllDeclareInfo(int id)
        {
            this.id = id;
        }

        [JsonIgnore]
        public int UnknownAfterId;
        public int Flags;
        public int ReturnDataType;
        public string Name;
        public string Comment;
        public string NameInLibrary;
        public string LibraryFile;
        public VariableInfo[] Parameters;
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
                    LibraryFile = reader.ReadStringWithLengthPrefix(),
                    NameInLibrary = reader.ReadStringWithLengthPrefix(),
                    Parameters = VariableInfo.ReadVariables(reader)
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
                writer.WriteStringWithLengthPrefix(dllDeclare.LibraryFile);
                writer.WriteStringWithLengthPrefix(dllDeclare.NameInLibrary);
                VariableInfo.WriteVariables(writer, dllDeclare.Parameters);
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
