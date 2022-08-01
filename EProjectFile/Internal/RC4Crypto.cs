using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QIQI.EProjectFile.Internal
{
    internal class RC4Crypto : ICloneable
    {
        private byte[] status;
        private int i = 0;
        private int j = 0;
        public RC4Crypto(byte[] initialStatus)
        {
            status = initialStatus ?? throw new ArgumentNullException(nameof(initialStatus));
            status = (byte[])status.Clone();
        }
        public RC4Crypto(byte[] key, byte[] initialStatus)
        {
            status = initialStatus ?? throw new ArgumentNullException(nameof(initialStatus));
            status = (byte[])status.Clone();

            int p2 = 0;
            for (int p1 = 0; p1 < status.Length; p1++)
            {
                p2 = (p2 + status[p1] + key[p1 % key.Length]) % status.Length;
                byte temp = status[p1];
                status[p1] = status[p2];
                status[p2] = temp;
            }
        }
        public void Decode(byte[] data, long start, long length)
        {
            long end = start + length;
            for (var offset = start; offset < end; offset++)
            {
                i = (i + 1) % status.Length;
                j = (j + status[i]) % status.Length;
                byte temp = status[i];
                status[i] = status[j];
                status[j] = temp;
                if (data != null)
                {
                    data[offset] ^= status[(status[i] + status[j]) % status.Length];
                }
            }
        }
        public void Encode(byte[] data, long start, long length) => Decode(data, start, length);
        public void Decode(byte[] data) => Decode(data, 0, data.Length);
        public void Encode(byte[] data) => Encode(data, 0, data.Length);
        public void Skip(long length) => Decode(null, 0, length);
        public object Clone() => new RC4Crypto(status) { i = i, j = j };
    }
}
