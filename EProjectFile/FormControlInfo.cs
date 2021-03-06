﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace QIQI.EProjectFile
{
    public class FormControlInfo : FormElementInfo
    {
        [JsonIgnore]
        public byte[] UnknownBeforeName { get; set; }
        public string Comment { get; set; }
        [JsonIgnore]
        public int UnknownBeforeLeft { get; set; } // !=0
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        [JsonIgnore]
        public int UnknownBeforeParent { get; set; }
        public int Parent { get; set; }
        public int[] Children { get; set; }
        [JsonConverter(typeof(HexConverter))]
        public byte[] Cursor { get; set; }
        public string Tag { get; set; }
        [JsonIgnore]
        public int UnknownBeforeVisible { get; set; }
        [JsonIgnore]
        public int UnknownBeforeEvents { get; set; }
        /// <summary>
        /// 【仅用于不带有编辑信息的EC文件】事件处理程序映射表
        /// </summary>
        public KeyValuePair<int, int>[] Events { get; set; }
        [JsonIgnore]
        public byte[] UnknownBeforeExtensionData { get; set; }
        /// <summary>
        /// 控件特有数据
        /// </summary>
        [JsonConverter(typeof(HexConverter))]
        public byte[] ExtensionData { get; set; }
        internal static FormControlInfo ReadWithoutDataType(BinaryReader reader, Encoding encoding, int length)
        {
            var startPosition = reader.BaseStream.Position;
            var elem = new FormControlInfo() { };
            elem.UnknownBeforeName = reader.ReadBytes(20);
            elem.Name = reader.ReadCStyleString(encoding);
            elem.Comment = reader.ReadCStyleString(encoding);
            elem.UnknownBeforeLeft = reader.ReadInt32();
            elem.Left = reader.ReadInt32();
            elem.Top = reader.ReadInt32();
            elem.Width = reader.ReadInt32();
            elem.Height = reader.ReadInt32();
            elem.UnknownBeforeParent = reader.ReadInt32();
            elem.Parent = reader.ReadInt32();
            elem.Children = reader.ReadInt32sWithLengthPrefix();
            elem.Cursor = reader.ReadBytesWithLengthPrefix();
            elem.Tag = reader.ReadCStyleString(encoding);
            elem.UnknownBeforeVisible = reader.ReadInt32();
            {
                int showStatus = reader.ReadInt32();
                elem.Visible = (showStatus & 0x1) != 0;
                elem.Disable = (showStatus & 0x2) != 0;
            }
            elem.UnknownBeforeEvents = reader.ReadInt32();
            elem.Events = new object[reader.ReadInt32()].Select(x => new KeyValuePair<int, int>(reader.ReadInt32(), reader.ReadInt32())).ToArray();
            elem.UnknownBeforeExtensionData = reader.ReadBytes(20);
            elem.ExtensionData = reader.ReadBytes(length - (int)(reader.BaseStream.Position - startPosition));
            return elem;
        }
        protected override void WriteWithoutId(BinaryWriter writer, Encoding encoding)
        {
            writer.Write(DataType);
            writer.Write(UnknownBeforeName);
            writer.WriteCStyleString(encoding, Name);
            writer.WriteCStyleString(encoding, Comment);
            writer.Write(UnknownBeforeLeft);
            writer.Write(Left);
            writer.Write(Top);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(UnknownBeforeParent);
            writer.Write(Parent);
            writer.WriteInt32sWithLengthPrefix(Children);
            writer.WriteBytesWithLengthPrefix(Cursor);
            writer.WriteCStyleString(encoding, Tag);
            writer.Write(UnknownBeforeVisible);
            writer.Write((Visible ? 0x1 : 0) | (Disable ? 0x2 : 0));
            writer.Write(UnknownBeforeEvents);
            writer.Write(Events.Length);
            Array.ForEach(
                Events, 
                x =>
                {
                    writer.Write(x.Key);
                    writer.Write(x.Value);
                });
            writer.Write(UnknownBeforeExtensionData);
            writer.Write(ExtensionData);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
