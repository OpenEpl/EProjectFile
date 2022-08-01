using Newtonsoft.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public class FormMenuInfo : FormElementInfo
    {
        [JsonIgnore]
        public byte[] UnknownBeforeName { get; set; }
        public int HotKey { get; set; }
        public int Level { get; set; }
        public bool Selected { get; set; }
        public string Text { get; set; }
        /// <summary>
        /// 仅EC有效
        /// </summary>
        public int ClickEvent { get; set; }
        [JsonIgnore]
        public byte[] UnknownAfterClickEvent { get; set; }
        internal static FormMenuInfo ReadWithoutDataType(BinaryReader reader, Encoding encoding, int length)
        {
            var startPosition = reader.BaseStream.Position;
            var elem = new FormMenuInfo() { };
            elem.UnknownBeforeName = reader.ReadBytes(20);
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
            elem.UnknownAfterClickEvent = reader.ReadBytes(length - (int)(reader.BaseStream.Position - startPosition));
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
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
