using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace QIQI.EProjectFile
{
    public class FormInfo
    {
        public int Id { get; set; }
        public int MemoryAddress { get; set; }
        public int UnknownBeforeClass { get; set; }
        /// <summary>
        /// 对应的窗口程序集
        /// </summary>
        public int Class { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public FormElementInfo[] Elements { get; set; }
        public static FormInfo[] ReadForms(BinaryReader reader, Encoding encoding)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var memoryAddresss = reader.ReadInt32sWithFixedLength(count);
            var forms = new FormInfo[count];
            for (int i = 0; i < count; i++)
            {
                var form = new FormInfo()
                {
                    Id = ids[i],
                    MemoryAddress = memoryAddresss[i],
                    UnknownBeforeClass = reader.ReadInt32(),
                    Class = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(encoding),
                    Comment = reader.ReadStringWithLengthPrefix(encoding),
                    Elements = FormElementInfo.ReadFormElements(reader, encoding)
                };
                forms[i] = form;
            }
            return forms;
        }

        public static void WriteForms(BinaryWriter writer, Encoding encoding, FormInfo[] forms)
        {
            writer.Write(forms.Length * 8);
            Array.ForEach(forms, x => writer.Write(x.Id));
            Array.ForEach(forms, x => writer.Write(x.MemoryAddress));
            foreach (var form in forms)
            {
                writer.Write(form.UnknownBeforeClass);
                writer.Write(form.Class);
                writer.WriteStringWithLengthPrefix(encoding, form.Name);
                writer.WriteStringWithLengthPrefix(encoding, form.Comment);
                FormElementInfo.WriteFormElements(writer, encoding, form.Elements);
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
