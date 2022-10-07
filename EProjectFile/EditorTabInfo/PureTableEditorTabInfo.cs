using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.EditorTabInfo
{
    public abstract class PureTableEditorTabInfo : IEditorTabInfo
    {
        protected class KeyImplForPureTable<TEditorTabInfo> : IEditorTabInfoKey<TEditorTabInfo> where TEditorTabInfo : PureTableEditorTabInfo, new()
        {
            public KeyImplForPureTable(byte type)
            {
                TypeId = type;
            }

            public byte TypeId { get; }

            public TEditorTabInfo Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                using BinaryReader reader = new BinaryReader(new MemoryStream(data, false), encoding);
                if (reader.ReadByte() != TypeId)
                {
                    throw new Exception($"Mismatched type for {typeof(TEditorTabInfo).Name}");
                }
                var that = new TEditorTabInfo()
                {
                    Offset = reader.ReadInt32() & 0x7FFFFFFF,
                    ColumnInTable = reader.ReadByte(),
                    SelectionStart = reader.ReadInt32(),
                    SelectionCurrent = reader.ReadInt32(),
                    SelectionEndpoints = new List<int>()
                };
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    that.SelectionEndpoints.Add(reader.ReadInt32());
                }
                return that;
            }
        }

        public abstract byte TypeId { get; }

        /// <summary>
        /// 对于表格元素为行索引
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// 对于表格元素为列索引
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

        /// <summary>
        /// 标记一个选择点，以开始、结束交叉出现，可以存在多个不连续的选择区
        /// </summary>
        /// <seealso cref="Offset"/>
        public List<int> SelectionEndpoints { get; set; }

        public void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            writer.Write(TypeId);
            writer.Write(Offset | 0x80000000);
            writer.Write(ColumnInTable);
            writer.Write(SelectionStart);
            writer.Write(SelectionCurrent);
            if (SelectionEndpoints != null)
            {
                foreach (var item in SelectionEndpoints)
                {
                    writer.Write(item);
                }
            }
        }
    }
    public class StructEditorTabInfo : PureTableEditorTabInfo
    {
        public static readonly IEditorTabInfoKey<StructEditorTabInfo> Key = new KeyImplForPureTable<StructEditorTabInfo>(2);
        public override byte TypeId => Key.TypeId;
    }
    public class GlobalVariableEditorTabInfo : PureTableEditorTabInfo
    {
        public static readonly IEditorTabInfoKey<GlobalVariableEditorTabInfo> Key = new KeyImplForPureTable<GlobalVariableEditorTabInfo>(3);
        public override byte TypeId => Key.TypeId;
    }
    public class DllDeclareEditorTabInfo : PureTableEditorTabInfo
    {
        public static readonly IEditorTabInfoKey<DllDeclareEditorTabInfo> Key = new KeyImplForPureTable<DllDeclareEditorTabInfo>(4);
        public override byte TypeId => Key.TypeId;
    }
    public class ConstantEditorTabInfo : PureTableEditorTabInfo
    {
        public static readonly IEditorTabInfoKey<ConstantEditorTabInfo> Key = new KeyImplForPureTable<ConstantEditorTabInfo>(6);
        public override byte TypeId => Key.TypeId;
    }
    public class ImageResourceEditorTabInfo : PureTableEditorTabInfo
    {
        public static readonly IEditorTabInfoKey<ImageResourceEditorTabInfo> Key = new KeyImplForPureTable<ImageResourceEditorTabInfo>(7);
        public override byte TypeId => Key.TypeId;
    }
    public class SoundResourceEditorTabInfo : PureTableEditorTabInfo
    {
        public static readonly IEditorTabInfoKey<SoundResourceEditorTabInfo> Key = new KeyImplForPureTable<SoundResourceEditorTabInfo>(8);
        public override byte TypeId => Key.TypeId;
    }
}
