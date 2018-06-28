using System.IO;
using Newtonsoft.Json;

namespace QIQI.EProjectFile
{
    public class VariableInfo:IHasId
    {
        private int id;
        public int Id => id;
        public VariableInfo(int id)
        {
            this.id = id;
        }
        public int DataType;
        public int Flags;
        public int[] UBound;
        public string Name;
        public string Comment;
        public static VariableInfo[] ReadVariables(BinaryReader r)
        {
            return r.ReadBlocksWithIdAndOffest((reader, id) =>
                new VariableInfo(id)
                {
                    DataType = reader.ReadInt32(),
                    Flags = reader.ReadInt16(),
                    UBound = reader.ReadInt32sWithFixedLength(reader.ReadByte()),
                    Name = reader.ReadCStyleString(),
                    Comment = reader.ReadCStyleString()
                }
            );
        }
        public static void WriteVariables(BinaryWriter w, VariableInfo[] variables)
        {
            w.WriteBlocksWithIdAndOffest(variables, (writer, elem) =>
                {
                    writer.Write(elem.DataType);
                    writer.Write((short)elem.Flags);
                    if (elem.UBound==null)
                    {
                        writer.Write((byte)0);
                    }
                    else
                    {
                        writer.Write((byte)elem.UBound.Length);
                        writer.WriteInt32sWithoutLengthPrefix(elem.UBound);
                    }
                    writer.WriteCStyleString(elem.Name);
                    writer.WriteCStyleString(elem.Comment);
                });
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
