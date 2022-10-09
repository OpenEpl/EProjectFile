using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace QIQI.EProjectFile.Encryption
{
    public abstract partial class EplSecret
    {
        public sealed class EStd: EplSecret, IEquatable<EStd>
        {
            private class FactoryImpl : IEplSecretFactory<EStd>
            {
                public EStd Create(byte[] key)
                {
                    var status = new RC4Crypto(key, 256).UnsafeGetStatus();
                    return new EStd(CalculateSecretID(key), Unsafe.As<byte[], ImmutableArray<byte>>(ref status));
                }

                public EStd Create(string key)
                {
                    return Create(Encoding.GetEncoding("gbk").GetBytes(key));
                }

                private static ImmutableArray<byte> CalculateSecretID(byte[] key)
                {
                    byte[] hash = MD5.Create().ComputeHash(key);
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = hash.Length - 1; i >= 0; i--)
                    {
                        stringBuilder.Append(hash[i].ToString("x2"));
                    }

                    var bytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
                    return Unsafe.As<byte[], ImmutableArray<byte>>(ref bytes);
                }
            }

            public static readonly IEplSecretFactory<EStd> Factory = new FactoryImpl();

            public override ImmutableArray<byte> SecretId { get; }
            public override ImmutableArray<byte> IV { get; }

            public EStd(ImmutableArray<byte> secretId, ImmutableArray<byte> iv)
            {
                SecretId = secretId;
                IV = iv;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as EStd);
            }

            public bool Equals(EStd other)
            {
                return !(other is null) &&
                       SecretId.Equals(other.SecretId) &&
                       IV.Equals(other.IV);
            }

            public override int GetHashCode()
            {
                int hashCode = -1032396774;
                hashCode = hashCode * -1521134295 + SecretId.GetHashCode();
                hashCode = hashCode * -1521134295 + IV.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(EStd left, EStd right)
            {
                return EqualityComparer<EStd>.Default.Equals(left, right);
            }

            public static bool operator !=(EStd left, EStd right)
            {
                return !(left == right);
            }
        }
    }
}
