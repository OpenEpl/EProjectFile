using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile.Context
{
    public struct BlockByteifierContext
    {
        public BlockByteifierContext(Encoding encoding)
        {
            this.Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public Encoding Encoding;

        public byte[] Collect(Action<BinaryWriter> writeTo)
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream(), Encoding))
            {
                writeTo(writer);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }
    }
}
