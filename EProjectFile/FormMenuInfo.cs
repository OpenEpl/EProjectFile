using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile
{
    public class FormMenuInfo : FormElementInfo
    {
        private static readonly ImmutableArray<byte> Zero16Bytes = ImmutableArray.Create(new byte[16]);
        private static readonly ImmutableArray<byte> Zero20Bytes = ImmutableArray.Create(new byte[20]);

        public override int Id { get; }

        public FormMenuInfo(int id)
        {
            this.Id = id;
        }

        [JsonIgnore]
        public ImmutableArray<byte> UnknownBeforeName { get; set; } = Zero20Bytes;
        public int HotKey { get; set; }
        public int Level { get; set; }
        public bool Selected { get; set; }
        public string Text { get; set; }
        /// <summary>
        /// 仅EC有效
        /// </summary>
        public int ClickEvent { get; set; }
        [JsonIgnore]
        public ImmutableArray<byte> UnknownAfterClickEvent { get; set; } = Zero16Bytes;
        internal static FormMenuInfo ReadWithoutDataType(BinaryReader reader, Encoding encoding, int id, int length)
        {
            var startPosition = reader.BaseStream.Position;
            var elem = new FormMenuInfo(id);
            elem.UnknownBeforeName = reader.ReadImmutableBytes(20) switch
            {
                var x when x.SequenceEqual(Zero20Bytes) => Zero20Bytes,
                var x => x
            };
            elem.Name = reader.ReadCStyleString(encoding);
            reader.ReadCStyleString(encoding); // 菜单没有Comment
            elem.HotKey = reader.ReadInt32();
            elem.Level = reader.ReadInt32();
            {
                int showStatus = reader.ReadInt32();
                elem.Visible = (showStatus & 0x1) == 0;
                elem.Disable = (showStatus & 0x2) != 0;
                elem.Selected = (showStatus & 0x4) != 0;
                if ((showStatus & 0xFFFFFFF8) != 0)
                {
                    throw new Exception($"Unknown flag for show status of the menu is found, value = 0x{showStatus:X8}");
                }
            }
            elem.Text = reader.ReadCStyleString(encoding);
            elem.ClickEvent = reader.ReadInt32();
            elem.UnknownAfterClickEvent = reader.ReadImmutableBytes(length - (int)(reader.BaseStream.Position - startPosition)) switch
            {
                var x when x.SequenceEqual(Zero16Bytes) => Zero16Bytes,
                var x => x
            }; ;
            return elem;
        }
        protected override void WriteWithoutId(BinaryWriter writer, Encoding encoding)
        {
            writer.Write(DataType);
            writer.Write(UnknownBeforeName);
            writer.WriteCStyleString(encoding, Name);
            writer.WriteCStyleString(encoding, "");
            writer.Write(HotKey);
            writer.Write(Level);
            writer.Write((Visible ? 0 : 0x1) | (Disable ? 0x2 : 0) | (Selected ? 0x4 : 0));
            writer.WriteCStyleString(encoding, Text);
            writer.Write(ClickEvent);
            writer.Write(UnknownAfterClickEvent);
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
