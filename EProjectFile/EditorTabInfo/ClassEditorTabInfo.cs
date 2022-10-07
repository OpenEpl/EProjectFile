using QIQI.EProjectFile.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.EditorTabInfo
{
    public class ClassEditorTabInfo : IEditorTabInfo
    {
        private class KeyImpl : IEditorTabInfoKey<ClassEditorTabInfo>
        {
            public byte TypeId => 1;

            public ClassEditorTabInfo Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    if (reader.ReadByte() != TypeId)
                    {
                        throw new Exception($"Mismatched type for {nameof(ClassEditorTabInfo)}");
                    }
                    var that = new ClassEditorTabInfo()
                    {
                        ClassId = reader.ReadInt32(),
                        ElemId = reader.ReadInt16(),
                        Offset = reader.ReadInt32() & 0x7FFFFFFF,
                        ColumnInTable = reader.ReadByte(),
                        SelectionStart = reader.ReadInt32(),
                        SelectionCurrent = reader.ReadInt32(),
                        SelectionEndpoints = new List<ClassEditorEndpoint>()
                    };
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var elemId = reader.ReadInt16();
                        var offset = reader.ReadInt32();
                        that.SelectionEndpoints.Add(new ClassEditorEndpoint()
                        {
                            ElemId = elemId,
                            Offset = offset
                        });
                    }
                    return that;
                });
            }
        }
        public static readonly IEditorTabInfoKey<ClassEditorTabInfo> Key = new KeyImpl();
        public byte TypeId => Key.TypeId;

        /// <summary>
        /// 对于表格元素为行索引，对于代码元素为所在行的代码数据位置（字节）
        /// </summary>
        /// <seealso cref="ClassEditorEndpoint.Offset"/>
        public int Offset { get; set; }

        /// <summary>
        /// 对于表格元素为列索引，对于代码元素为<c>0</c>
        /// </summary>
        public byte ColumnInTable { get; set; }

        /// <summary>
        /// 使选择方向不同，可能大于 <see cref="SelectionCurrent"/> 也可能小于，若两者相等表示无行内选区（可能有多行选择存在）
        /// </summary>
        /// <remarks>多行选择时此值与 <see cref="SelectionCurrent"/> 相等，选择点以 <see cref="SelectionEndpoints"/> 为准</remarks>
        public int SelectionStart { get; set; }

        /// <summary>
        /// 当前光标位置
        /// </summary>
        /// <remarks>多行选择时此值与 <see cref="SelectionStart"/> 相等，选择点以 <see cref="SelectionEndpoints"/> 为准</remarks>
        public int SelectionCurrent { get; set; }

        public int ClassId { get; set; }

        /// <summary>
        /// -N 表示第 N 个表格，自然数表示对应的子程序索引（从0开始）
        /// </summary>
        /// <seealso cref="ClassEditorEndpoint.ElemId"/>
        public short ElemId { get; set; }

        /// <summary>
        /// 标记一个选择点，以开始、结束交叉出现，可以存在多个不连续的选择区
        /// </summary>
        public List<ClassEditorEndpoint> SelectionEndpoints { get; set; }

        public void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            writer.Write(20 + (SelectionEndpoints?.Count ?? 0) * 6);
            writer.Write(TypeId);
            writer.Write(ClassId);
            writer.Write(ElemId);
            writer.Write(Offset | unchecked((int)0x80000000));
            writer.Write(ColumnInTable);
            writer.Write(SelectionStart);
            writer.Write(SelectionCurrent);
            if (SelectionEndpoints != null)
            {
                foreach (var item in SelectionEndpoints)
                {
                    writer.Write(item.ElemId);
                    writer.Write(item.Offset);
                }
            }
        }
    }
    public struct ClassEditorEndpoint
    {
        /// <summary>
        /// -N 表示第 N 个表格，自然数表示对应的子程序索引（从0开始）
        /// </summary>
        /// <seealso cref="ClassEditorTabInfo.ElemId"/>
        public short ElemId { get; set; }

        /// <summary>
        /// 对于表格元素为行索引，对于代码元素为所在行的代码数据位置（字节）
        /// </summary>
        /// <seealso cref="ClassEditorTabInfo.Offset"/>
        public int Offset { get; set; }
    }
}
