using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using QIQI.EProjectFile.Sections;
using System.IO;

namespace QIQI.EProjectFile.Internal
{
    internal class TextCodeUtils
    {
        private TextCodeUtils() => throw new NotSupportedException();
        public static void WriteDefinitionCode(TextWriter writer, int indent, string type, params string[] items)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("声明类型不能为空", nameof(type));
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            writer.Write(".");
            writer.Write(type);
            if (items != null & items.Length != 0)
            {
                var count = items.Length;
                while (count > 0 && string.IsNullOrEmpty(items[count - 1]))
                    count--;
                if (count == 0) return;
                writer.Write(" ");
                writer.Write(string.Join(", ", items.Take(count)));
            }
        }
        private static void JoinAndWriteCode<T>(IEnumerable<T> items, string separator, TextWriter writer, Action<T> writeTo) where T : IToTextCodeAble
        {
            if (items == null) return;
            bool first = true;
            foreach (var item in items)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.Write(separator);
                }
                writeTo(item);
            }
        }
        public static void JoinAndWriteCode(IEnumerable<IToTextCodeAble> items, string separator, IdToNameMap nameMap, TextWriter writer, int indent)
        {
            JoinAndWriteCode(items, separator, writer, x => x.ToTextCode(nameMap, writer, indent));
        }
        public static void JoinAndWriteCode(IEnumerable<ClassInfo> items, string separator, CodeSection codeSection, IdToNameMap nameMap, TextWriter writer, int indent, bool writeCode = true)
        {
            JoinAndWriteCode(items, separator, writer, x => x.ToTextCode(nameMap, writer, indent, codeSection, writeCode));
        }
        public static void JoinAndWriteCode(IEnumerable<MethodInfo> items, string separator, IdToNameMap nameMap, TextWriter writer, int indent, bool writeCode)
        {
            JoinAndWriteCode(items, separator, writer, x => x.ToTextCode(nameMap, writer, indent, writeCode));
        }
    }
}
