using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 语句块
    /// </summary>
    public class StatementBlock : IList<Statement>, IToTextCodeAble
    {
        private List<Statement> statements = new List<Statement>();

        public int Count => ((IList<Statement>)statements).Count;

        public bool IsReadOnly => ((IList<Statement>)statements).IsReadOnly;

        public Statement this[int index] { get => ((IList<Statement>)statements)[index]; set => ((IList<Statement>)statements)[index] = value; }

        public StatementBlock()
        {

        }
        internal void WriteTo(MethodCodeDataWriterArgs a)
        {
            statements.ForEach(x => x.WriteTo(a));
        }
        public MethodCodeData ToCodeData()
        {
            BinaryWriter newWriter() => new BinaryWriter(new MemoryStream());
            byte[] getBytes(BinaryWriter x) => ((MemoryStream)x.BaseStream).ToArray();
            using (BinaryWriter
                lineOffest = newWriter(),
                blockOffest = newWriter(),
                methodReference = newWriter(),
                variableReference = newWriter(),
                constantReference = newWriter(),
                expressionData = newWriter())
            {
                var a = new MethodCodeDataWriterArgs
                {
                    LineOffest = lineOffest,
                    BlockOffest = blockOffest,
                    MethodReference = methodReference,
                    VariableReference = variableReference,
                    ConstantReference = constantReference,
                    ExpressionData = expressionData
                };
                WriteTo(a);
                return new MethodCodeData
                {
                    LineOffest = getBytes(lineOffest),
                    BlockOffest = getBytes(blockOffest),
                    MethodReference = getBytes(methodReference),
                    VariableReference = getBytes(variableReference),
                    ConstantReference = getBytes(constantReference),
                    ExpressionData = getBytes(expressionData)
                };
            }
        }
        public void ToTextCode(IdToNameMap nameMap, StringBuilder result, int indent = 0)
        {
            TextCodeUtils.WriteJoinCode(this, Environment.NewLine, nameMap, result, indent);
        }
        public sealed override string ToString() => this.ToTextCode(IdToNameMap.Empty);

        public int IndexOf(Statement item)
        {
            return ((IList<Statement>)statements).IndexOf(item);
        }

        public void Insert(int index, Statement item)
        {
            ((IList<Statement>)statements).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Statement>)statements).RemoveAt(index);
        }

        public void Add(Statement item)
        {
            ((IList<Statement>)statements).Add(item);
        }

        public void Clear()
        {
            ((IList<Statement>)statements).Clear();
        }

        public bool Contains(Statement item)
        {
            return ((IList<Statement>)statements).Contains(item);
        }

        public void CopyTo(Statement[] array, int arrayIndex)
        {
            ((IList<Statement>)statements).CopyTo(array, arrayIndex);
        }

        public bool Remove(Statement item)
        {
            return ((IList<Statement>)statements).Remove(item);
        }

        public IEnumerator<Statement> GetEnumerator()
        {
            return ((IList<Statement>)statements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Statement>)statements).GetEnumerator();
        }
    }
}
