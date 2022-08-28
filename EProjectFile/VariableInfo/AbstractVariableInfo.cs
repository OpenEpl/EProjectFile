using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using QIQI.EProjectFile.Internal;

namespace QIQI.EProjectFile
{
    public abstract class AbstractVariableInfo : IHasId, IToTextCodeAble
    {
        public int Id { get; }
        public virtual int[] UBound { get ; set ; }

        public AbstractVariableInfo(int id)
        {
            this.Id = id;
        }
        public int DataType { get; set; }
        public int Flags { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        internal static List<TElem> ReadVariables<TElem>(BinaryReader r, Encoding encoding, Func<int, TElem> newFunction) where TElem : AbstractVariableInfo
        {
            return r.ReadBlocksWithIdAndOffest((reader, id) =>
            {
                var x = newFunction(id);
                x.DataType = reader.ReadInt32();
                x.Flags = reader.ReadInt16();
                x.UBound = reader.ReadInt32sWithFixedLength(reader.ReadByte());
                x.Name = reader.ReadCStyleString(encoding);
                x.Comment = reader.ReadCStyleString(encoding);
                return x;
            });
        }
        internal static void WriteVariables<TElem>(BinaryWriter w, Encoding encoding, List<TElem> variables) where TElem : AbstractVariableInfo
        {
            w.WriteBlocksWithIdAndOffest(
                encoding,
                variables,
                (writer, elem) =>
                {
                    writer.Write(elem.DataType);
                    writer.Write((short)elem.Flags);
                    if (elem.UBound == null)
                    {
                        writer.Write((byte)0);
                    }
                    else
                    {
                        writer.Write((byte)elem.UBound.Length);
                        writer.WriteInt32sWithoutLengthPrefix(elem.UBound);
                    }
                    writer.WriteCStyleString(encoding, elem.Name);
                    writer.WriteCStyleString(encoding, elem.Comment);
                });
        }
        public abstract void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0);
    }
}
