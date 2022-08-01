using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using QIQI.EProjectFile.Internal;

namespace QIQI.EProjectFile
{
    public class FormInfo: IHasId, IHasMemoryAddress
    {
        public int Id { get; }

        public FormInfo(int id)
        {
            this.Id = id;
        }

        public int MemoryAddress { get; set; }
        public int UnknownBeforeClass { get; set; }
        /// <summary>
        /// 对应的窗口程序集
        /// </summary>
        public int Class { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public FormElementInfo[] Elements { get; set; }
        public static FormInfo[] ReadForms(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndMemoryAddress((reader, id, memoryAddress) => new FormInfo(id)
            {
                MemoryAddress = memoryAddress,
                UnknownBeforeClass = reader.ReadInt32(),
                Class = reader.ReadInt32(),
                Name = reader.ReadStringWithLengthPrefix(encoding),
                Comment = reader.ReadStringWithLengthPrefix(encoding),
                Elements = FormElementInfo.ReadFormElements(reader, encoding)
            });
        }

        public static void WriteForms(BinaryWriter w, Encoding encoding, FormInfo[] forms)
        {
            w.WriteBlocksWithIdAndMemoryAddress(forms, (writer, elem) =>
            {
                writer.Write(elem.UnknownBeforeClass);
                writer.Write(elem.Class);
                writer.WriteStringWithLengthPrefix(encoding, elem.Name);
                writer.WriteStringWithLengthPrefix(encoding, elem.Comment);
                FormElementInfo.WriteFormElements(writer, encoding, elem.Elements);
            });
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
