using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Context
{
    public struct BlockParserContext
    {
        public BlockParserContext(byte[] data, Encoding encoding, bool cryptEC)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            CryptEC = cryptEC;
        }

        private readonly byte[] Data;
        public int DataLength => Data.Length;
        public readonly Encoding Encoding;
        public readonly bool CryptEC;

        public TResult Consume<TResult>(Func<BinaryReader, TResult> consumer)
        {
            using var reader = new BinaryReader(new MemoryStream(Data, false));
            return consumer(reader);
        }
    }
}
