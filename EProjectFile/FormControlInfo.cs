﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text;
using QIQI.EProjectFile.Internal;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile
{
    public class FormControlInfo : FormElementInfo
    {
        private static readonly ImmutableArray<byte> Zero20Bytes = ImmutableArray.Create(new byte[20]);

        public override int Id { get; }
        [JsonIgnore]
        public ImmutableArray<byte> UnknownBeforeName { get; set; } = Zero20Bytes;
        public string Comment { get; set; }
        /// <summary>
        /// 最后一次保存时易语言窗口设计器创建该组件后得到的 CWnd 对象的内存地址
        /// 在用于编辑文件的场景下，意义不大
        /// </summary>
        public int CWndAddress { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        [JsonIgnore]
        public int UnknownBeforeParent { get; set; }
        public int Parent { get; set; }
        public int[] Children { get; set; }
        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] Cursor { get; set; }
        public string Tag { get; set; }
        [JsonIgnore]
        public int UnknownBeforeVisible { get; set; }
        public bool TabStop { get; set; }
        /// <summary>
        /// 被锁定的组件在设计器中无法被修改位置（右键菜单可以“锁定”、“解除锁定”）
        /// </summary>
        public bool Locked { get; set; }
        public int TabIndex { get; set; }
        /// <summary>
        /// 【仅用于不带有编辑信息的EC文件】事件处理程序映射表
        /// </summary>
        public KeyValuePair<int, int>[] Events { get; set; }
        [JsonIgnore]
        public ImmutableArray<byte> UnknownBeforeExtensionData { get; set; } = Zero20Bytes;
        /// <summary>
        /// 控件特有数据
        /// </summary>
        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] ExtensionData { get; set; }

        public FormControlInfo(int id)
        {
            this.Id = id;
        }

        internal static FormControlInfo ReadWithoutDataType(BinaryReader reader, Encoding encoding, int id, int length)
        {
            var startPosition = reader.BaseStream.Position;
            var elem = new FormControlInfo(id);
            elem.UnknownBeforeName = reader.ReadImmutableBytes(20) switch
            {
                var x when x.SequenceEqual(Zero20Bytes) => Zero20Bytes,
                var x => x
            };
            elem.Name = reader.ReadCStyleString(encoding);
            elem.Comment = reader.ReadCStyleString(encoding);
            elem.CWndAddress = reader.ReadInt32();
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
                elem.TabStop = (showStatus & 0x4) != 0;
                elem.Locked = (showStatus & 0x10) != 0;
                if ((showStatus & 0xFFFFFFE8) != 0)
                {
                    throw new Exception($"Unknown flag for show status of the control is found, value = 0x{showStatus:X8}");
                }
            }
            elem.TabIndex = reader.ReadInt32();
            elem.Events = new object[reader.ReadInt32()].Select(x => new KeyValuePair<int, int>(reader.ReadInt32(), reader.ReadInt32())).ToArray();
            elem.UnknownBeforeExtensionData = reader.ReadImmutableBytes(20) switch
            {
                var x when x.SequenceEqual(Zero20Bytes) => Zero20Bytes,
                var x => x
            };
            elem.ExtensionData = reader.ReadBytes(length - (int)(reader.BaseStream.Position - startPosition));
            return elem;
        }
        protected override void WriteWithoutId(BinaryWriter writer, Encoding encoding)
        {
            writer.Write(DataType);
            writer.Write(UnknownBeforeName);
            writer.WriteCStyleString(encoding, Name);
            writer.WriteCStyleString(encoding, Comment);
            writer.Write(CWndAddress);
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
            writer.Write((Visible ? 0x1 : 0) | (Disable ? 0x2 : 0) | (TabStop ? 0x4 : 0) | (Locked ? 0x10 : 0));
            writer.Write(TabIndex);
            if (Events is null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(Events.Length);
                foreach (var x in Events)
                {
                    writer.Write(x.Key);
                    writer.Write(x.Value);
                }
            }
            writer.Write(UnknownBeforeExtensionData);
            writer.Write(ExtensionData);
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
