using System;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    internal struct MethodCodeDataWriterArgs
    {
        public Encoding Encoding;
        public BinaryWriter LineOffest;
        public BinaryWriter BlockOffest;
        public BinaryWriter MethodReference;
        public BinaryWriter VariableReference;
        public BinaryWriter ConstantReference;
        public BinaryWriter ExpressionData;
        public int Offest => (int)ExpressionData.BaseStream.Position;
        public IDisposable NewBlock(byte type)
        {
            return new BlockOffestHelper(this, type);
        }
        private struct BlockOffestHelper : IDisposable
        {
            private bool disposed;
            private MethodCodeDataWriterArgs a;
            private long posToFillEndOffest;
            public BlockOffestHelper(MethodCodeDataWriterArgs writers, byte type)
            {
                disposed = false;
                a = writers;
                a.BlockOffest.Write(type);
                a.BlockOffest.Write(a.Offest);
                posToFillEndOffest = a.BlockOffest.BaseStream.Position;
                a.BlockOffest.Write(0);
            }
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;

                    long curPos = a.BlockOffest.BaseStream.Position;
                    a.BlockOffest.BaseStream.Position = posToFillEndOffest;
                    a.BlockOffest.Write(a.Offest);
                    a.BlockOffest.BaseStream.Position = curPos;
                }
            }
        }

    }
}
