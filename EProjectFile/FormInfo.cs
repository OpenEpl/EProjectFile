using System;
using System.IO;
using Newtonsoft.Json;

namespace QIQI.EProjectFile
{
    public class FormInfo
    {
        public int Id { get; set; }
        public int UnknownAfterId { get; set; }
        public int UnknownBeforeClass { get; set; }
        /// <summary>
        /// 对应的窗口程序集
        /// </summary>
        public int Class { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public FormElementInfo[] Elements { get; set; }
        public static FormInfo[] ReadForms(BinaryReader reader)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var unknownsAfterIds = reader.ReadInt32sWithFixedLength(count);
            var forms = new FormInfo[count];
            for (int i = 0; i < count; i++)
            {
                var form = new FormInfo()
                {
                    Id = ids[i],
                    UnknownAfterId = unknownsAfterIds[i],
                    UnknownBeforeClass = reader.ReadInt32(),
                    Class = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(),
                    Comment = reader.ReadStringWithLengthPrefix(),
                    Elements = FormElementInfo.ReadFormElements(reader)
                };
                forms[i] = form;
            }
            return forms;
        }

        public static void WriteForms(BinaryWriter writer, FormInfo[] forms)
        {
            writer.Write(forms.Length * 8);
            Array.ForEach(forms, x => writer.Write(x.Id));
            Array.ForEach(forms, x => writer.Write(x.UnknownAfterId));
            foreach (var form in forms)
            {
                writer.Write(form.UnknownBeforeClass);
                writer.Write(form.Class);
                writer.WriteStringWithLengthPrefix(form.Name);
                writer.WriteStringWithLengthPrefix(form.Comment);
                FormElementInfo.WriteFormElements(writer, form.Elements);
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
