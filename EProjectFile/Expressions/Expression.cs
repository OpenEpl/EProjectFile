using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 表达式 基类
    /// </summary>
    public abstract class Expression : IToTextCodeAble
    {
        internal abstract void WriteTo(MethodCodeDataWriterArgs a);
        public sealed override string ToString() => this.ToTextCode(IdToNameMap.Empty);
        public abstract void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0);
    }
}
