using System.Linq;
using System.Text;
namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 语句 基类
    /// </summary>
    public abstract class Statement : IToTextCodeAble
    {
        internal abstract void WriteTo(MethodCodeDataWriterArgs a);
        public abstract void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0);
        public sealed override string ToString() => this.ToTextCode(IdToNameMap.Empty);
    }
}
