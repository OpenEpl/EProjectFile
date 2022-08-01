using QIQI.EProjectFile.Internal;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 参数列表，配合CallExpression用
    /// </summary>
    public class ParamListExpression : Expression, IList<Expression>
    {
        private readonly List<Expression> parameters = new List<Expression>();

        public int Count => parameters.Count;

        public bool IsReadOnly => ((IList<Expression>)parameters).IsReadOnly;

        public Expression this[int index] { get => parameters[index]; set => this.parameters[index] = value; }

        public void Add(Expression item)
        {
            parameters.Add(item ?? DefaultValueExpression.Instance);
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            parameters.ForEach(x => x.WriteTo(a));
            ParamListEnd.Instance.WriteTo(a);
        }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            writer.Write("(");
            TextCodeUtils.JoinAndWriteCode(this, ", ", nameMap, writer, indent);
            writer.Write(")");
        }

        public IEnumerator<Expression> GetEnumerator() => ((IEnumerable<Expression>)parameters).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Expression>)parameters).GetEnumerator();

        public int IndexOf(Expression item)
        {
            return parameters.IndexOf(item ?? DefaultValueExpression.Instance);
        }

        public void Insert(int index, Expression item)
        {
            parameters.Insert(index, item ?? DefaultValueExpression.Instance);
        }

        public void RemoveAt(int index)
        {
            parameters.RemoveAt(index);
        }

        public void Clear()
        {
            parameters.Clear();
        }

        public bool Contains(Expression item)
        {
            return parameters.Contains(item ?? DefaultValueExpression.Instance);
        }

        public void CopyTo(Expression[] array, int arrayIndex)
        {
            parameters.CopyTo(array, arrayIndex);
        }

        public bool Remove(Expression item)
        {
            return parameters.Remove(item ?? DefaultValueExpression.Instance);
        }
    }
}
