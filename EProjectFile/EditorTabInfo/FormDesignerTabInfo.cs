using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.EditorTabInfo
{
    public class FormDesignerTabInfo : IEditorTabInfo
    {
        private class KeyImpl : IEditorTabInfoKey<FormDesignerTabInfo>
        {
            public byte TypeId => 5;

            public FormDesignerTabInfo Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                using BinaryReader reader = new BinaryReader(new MemoryStream(data, false), encoding);
                if (reader.ReadByte() != TypeId)
                {
                    throw new Exception($"Mismatched type for {nameof(FormDesignerTabInfo)}");
                }
                var that = new FormDesignerTabInfo()
                {
                    FormId = reader.ReadInt32(),
                    UnitIds = new List<int>()
                };

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var unitId = reader.ReadInt32();
                    that.UnitIds.Add(unitId);
                }
                return that;
            }
        }
        public static readonly IEditorTabInfoKey<FormDesignerTabInfo> Key = new KeyImpl();
        public byte TypeId => Key.TypeId;
        public int FormId { get; set; }
        public List<int> UnitIds { get; set; }

        public void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            writer.Write(TypeId);
            writer.Write(FormId);
            if (UnitIds != null)
            {
                foreach (var unitId in UnitIds)
                {
                    writer.Write(unitId);
                }
            }
        }
    }
}
