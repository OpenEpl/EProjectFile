using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace QIQI.EProjectFile.Internal
{
    internal static class StreamWithImmutableExtension
    {
        public static ImmutableArray<byte> ReadImmutableBytes(this BinaryReader reader, int count)
        {
            var data = reader.ReadBytes(count);
            return Unsafe.As<byte[], ImmutableArray<byte>>(ref data);
        }
        public static void Write(this BinaryWriter writer, ImmutableArray<byte> data)
        {
            writer.Write(Unsafe.As<ImmutableArray<byte>, byte[]>(ref data));
        }
    }
}
