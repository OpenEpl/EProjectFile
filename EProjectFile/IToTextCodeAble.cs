using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public interface IToTextCodeAble
    {
        /// <summary>
        /// 开头含缩进，结尾不含换行
        /// </summary>
        /// <param name="nameMap"></param>
        /// <param name="writer"></param>
        /// <param name="indent"></param>
        void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0);
    }
    public static class ExtensionForIToTextCodeAble
    {
        public static string ToTextCode(this IToTextCodeAble target, IdToNameMap nameMap, int indent = 0)
        {
            var writer = new StringWriter();
            target.ToTextCode(nameMap, writer, indent);
            return writer.ToString();
        }
    }
}
