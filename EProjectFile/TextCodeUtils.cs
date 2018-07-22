using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
namespace QIQI.EProjectFile
{
    internal class TextCodeUtils
    {
        private TextCodeUtils() => throw new NotSupportedException();
        public static void WriteDefinedCode(StringBuilder builder, int indent, string type, params string[] items)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("声明类型不能为空", nameof(type));
            for (int i = 0; i < indent; i++)
                builder.Append("    ");
            builder.Append(".");
            builder.Append(type);
            if (items != null & items.Length != 0)
            {
                var count = items.Length;
                while (count > 0 && string.IsNullOrEmpty(items[count - 1]))
                    count--;
                if (count == 0) return;
                builder.Append(" ");
                builder.Append(string.Join(", ", items.Take(count)));
            }
        }
        private static void WriteJoinCode<T>(IEnumerable<T> items, string separator, StringBuilder builder, Action<T> writeTo) where T : IToTextCodeAble
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
                    builder.Append(separator);
                }
                writeTo(item);
            }
        }
        public static void WriteJoinCode(IEnumerable<IToTextCodeAble> items, string separator, IdToNameMap nameMap, StringBuilder builder, int indent)
        {
            WriteJoinCode(items, separator, builder, x => x.ToTextCode(nameMap, builder, indent));
        }
        public static void WriteJoinCode(IEnumerable<ClassInfo> items, string separator, CodeSectionInfo codeSection, IdToNameMap nameMap, StringBuilder builder, int indent, bool writeCode = true)
        {
            WriteJoinCode(items, separator, builder, x => x.ToTextCode(nameMap, builder, indent, codeSection, writeCode));
        }
        public static void WriteJoinCode(IEnumerable<MethodInfo> items, string separator, IdToNameMap nameMap, StringBuilder builder, int indent, bool writeCode)
        {
            WriteJoinCode(items, separator, builder, x => x.ToTextCode(nameMap, builder, indent, writeCode));
        }
    }
}
