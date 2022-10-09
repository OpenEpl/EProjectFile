using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace QIQI.EProjectFile.Internal
{
    internal class RC4Crypto : ICloneable
    {
        private readonly byte[] status;
        private int i = 0;
        private int j = 0;
        /// <param name="key">It can be null</param>
        /// <param name="initialStatus">It cannot be null</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RC4Crypto(byte[] key, byte[] initialStatus)
        {
            status = (byte[])initialStatus?.Clone() ?? throw new ArgumentNullException(nameof(initialStatus));
            EmitKey(key);
        }
        /// <param name="key">It can be null</param>
        /// <param name="initialStatus">It cannot be null</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RC4Crypto(ImmutableArray<byte> key, ImmutableArray<byte> initialStatus)
        {
            if (initialStatus.IsDefault)
            {
                throw new ArgumentException(nameof(initialStatus));
            }
            status = initialStatus.ToArray();
            EmitKey(key);
        }
        public RC4Crypto(byte[] key, int statusLength)
        {
            status = new byte[statusLength];
            for (i = 0; i < statusLength; i++)
            {
                status[i] = unchecked((byte)i);
            }
            EmitKey(key);
        }
        public RC4Crypto(ImmutableArray<byte> key, int statusLength)
        {
            status = new byte[statusLength];
            for (i = 0; i < statusLength; i++)
            {
                status[i] = unchecked((byte)i);
            }
            EmitKey(key);
        }
        private void EmitKey(byte[] key) => EmitKey(Unsafe.As<byte[], ImmutableArray<byte>>(ref key));
        private void EmitKey(ImmutableArray<byte> key)
        {
            if (key.IsDefaultOrEmpty)
            {
                return;
            }
            int p2 = 0;
            for (int p1 = 0; p1 < status.Length; p1++)
            {
                p2 = (p2 + status[p1] + key[p1 % key.Length]) % status.Length;
                (status[p2], status[p1]) = (status[p1], status[p2]);
            }
        }
        public void Decode(byte[] data, long start, long length)
        {
            long end = start + length;
            for (var offset = start; offset < end; offset++)
            {
                i = (i + 1) % status.Length;
                j = (j + status[i]) % status.Length;
                (status[j], status[i]) = (status[i], status[j]);
                if (data != null)
                {
                    data[offset] ^= status[(status[i] + status[j]) % status.Length];
                }
            }
        }
        /// <remarks>do not modify the return value</remarks>
        internal byte[] UnsafeGetStatus()
        {
            return status;
        }
        public void Encode(byte[] data, long start, long length) => Decode(data, start, length);
        public void Decode(byte[] data) => Decode(data, 0, data.Length);
        public void Encode(byte[] data) => Encode(data, 0, data.Length);
        public void Skip(long length) => Decode(null, 0, length);
        public object Clone() => new RC4Crypto(null, status) { i = i, j = j };
    }
}
