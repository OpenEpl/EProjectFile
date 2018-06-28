using Newtonsoft.Json;
using System.IO;

namespace QIQI.EProjectFile
{
    public class FormMenuInfo : FormElementInfo
    {
        [JsonIgnore]
        public byte[] UnknownBeforeName;
        public int HotKey;
        public int Level;
        public bool Selected;
        public string Text;
        public int ClickEvent;//仅EC有效
        [JsonIgnore]
        public byte[] UnknownAfterClickEvent;
        internal static FormMenuInfo ReadWithoutDataType(BinaryReader reader, int length)
        {
            var startPosition = reader.BaseStream.Position;
            var elem = new FormMenuInfo() { };
            elem.UnknownBeforeName = reader.ReadBytes(20);
            elem.Name = reader.ReadCStyleString();
            reader.ReadCStyleString();//菜单没有Comment
            elem.HotKey = reader.ReadInt32();
            elem.Level = reader.ReadInt32();
            {
                int showStatus = reader.ReadInt32();
                elem.Visible = (showStatus & 0x1) == 0;
                elem.Disable = (showStatus & 0x2) != 0;
                elem.Selected = (showStatus & 0x4) != 0;
            }
            elem.Text = reader.ReadCStyleString();
            elem.ClickEvent = reader.ReadInt32();
            elem.UnknownAfterClickEvent = reader.ReadBytes(length - (int)(reader.BaseStream.Position - startPosition));
            return elem;
        }
        protected override void WriteWithoutId(BinaryWriter writer)
        {
            writer.Write(DataType);
            writer.Write(UnknownBeforeName);
            writer.WriteCStyleString(Name);
            writer.WriteCStyleString("");
            writer.Write(HotKey);
            writer.Write(Level);
            writer.Write((Visible ? 0 : 0x1) | (Disable ? 0x2 : 0) | (Selected ? 0x4 : 0));
            writer.WriteCStyleString(Text);
            writer.Write(ClickEvent);
            writer.Write(UnknownAfterClickEvent);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
