using System;
using System.Collections.Generic;
using System.Text;

namespace QIQI.EProjectFile.Internal
{
    internal static class BytesUtils
    {
        public static byte[] HexToBytes(string src)
        {
            byte[] result = new byte[src.Length / 2];
            for (int i = 0, c = 0; i < src.Length; i += 2, c++)
            {
                result[c] = Convert.ToByte(src.Substring(i, 2), 16);
            }
            return result;
        }

        public static string BytesToHex(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 2);
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("X2"));
                }
            }
            return sb.ToString();
        }

    }
}
