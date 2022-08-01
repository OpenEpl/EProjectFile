using QIQI.EProjectFile.Internal;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Expressions
{
    /// <summary>
    /// 数组字面量（常量集），包括各种类型数组、字节集等
    /// </summary>
    public class ArrayLiteralExpression : Expression, IList<Expression>
    {
        private readonly List<Expression> items = new List<Expression>();

        public Expression this[int index] { get => items[index]; set => items[index] = value; }

        public int Count => items.Count;

        public bool IsReadOnly => ((IList<Expression>)items).IsReadOnly;

        public void Add(Expression item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(Expression item)
        {
            return items.Contains(item);
        }

        public void CopyTo(Expression[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            return ((IList<Expression>)items).GetEnumerator();
        }

        public int IndexOf(Expression item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, Expression item)
        {
            items.Insert(index, item);
        }

        public bool Remove(Expression item)
        {
            return items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        public override void ToTextCode(IdToNameMap nameMap, TextWriter writer, int indent = 0)
        {
            writer.Write("{");
            TextCodeUtils.JoinAndWriteCode(this, ", ", nameMap, writer, indent);
            writer.Write("}");
        }
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            a.ExpressionData.Write((byte)0x1F);
            items.ForEach(x => x.WriteTo(a));
            ArrayLiteralEnd.Instance.WriteTo(a);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Expression>)items).GetEnumerator();
        }
    }
}
