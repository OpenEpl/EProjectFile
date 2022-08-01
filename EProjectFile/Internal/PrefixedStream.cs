using System;
using System.IO;

namespace QIQI.EProjectFile
{
    internal class PrefixedStream: Stream
    {
        public Stream BaseStream { get; }
        public byte[] Prefix { get; }
        private int offsetOfPrefix;
        public PrefixedStream(Stream stream, byte[] prefix)
        {
            this.BaseStream = stream;
            this.Prefix = prefix;
        }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                BaseStream.Dispose();
            }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int copiedLength = Math.Min(Prefix.Length - offsetOfPrefix, count);
            if (copiedLength > 0)
            {
                Array.Copy(Prefix, offsetOfPrefix, buffer, offset, copiedLength);
                offsetOfPrefix += copiedLength;
                offset += copiedLength;
                count -= copiedLength;
                if (count == 0)
                {
                    return copiedLength;
                }
            }
            return BaseStream.Read(buffer, offset, count) + copiedLength;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int copiedLength = Math.Min(Prefix.Length - offsetOfPrefix, count);
            if (copiedLength > 0)
            {
                Array.Copy(buffer, offset, Prefix, offsetOfPrefix, copiedLength);
                offsetOfPrefix += copiedLength;
                offset += copiedLength;
                count -= copiedLength;
                if (count == 0)
                {
                    return;
                }
            }
            BaseStream.Write(buffer, offset, count);
        }
    }
}
