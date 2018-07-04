using System.IO;
using Newtonsoft.Json;

namespace QIQI.EProjectFile
{
    public class VariableInfo:IHasId
    {
        public int Id { get; }

        public VariableInfo(int id)
        {
            this.Id = id;
        }
        public int DataType;
        public int Flags;
        /// <summary>
        /// 仅局部变量有效
        /// </summary>
        public bool Static { get => (Flags & 0x1) != 0; set => Flags = (Flags & ~0x1) | (value ? 0x1 : 0); }
        /// <summary>
        /// 仅参数、自定义类型成员有效
        /// </summary>
        public bool ByRef { get => (Flags & 0x2) != 0; set => Flags = (Flags & ~0x2) | (value ? 0x2 : 0); }
        /// <summary>
        /// 仅参数有效
        /// </summary>
        public bool OptionalParameter { get => (Flags & 0x4) != 0; set => Flags = (Flags & ~0x4) | (value ? 0x4 : 0); }
        /// <summary>
        /// 仅参数有效
        /// </summary>
        public bool ArrayParameter { get => (Flags & 0x8) != 0; set => Flags = (Flags & ~0x8) | (value ? 0x8 : 0); }
        /// <summary>
        /// 仅全局变量有效
        /// </summary>
        public bool Public { get => (Flags & 0x100) != 0; set => Flags = (Flags & ~0x100) | (value ? 0x100 : 0); }
        /// <summary>
        /// 仅变量、自定义类型成员有效
        /// </summary>
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
