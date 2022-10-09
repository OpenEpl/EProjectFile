using System.Collections.Immutable;

namespace QIQI.EProjectFile.Encryption
{
    public abstract partial class EplSecret
    {
        public abstract ImmutableArray<byte> SecretId { get; }
        public abstract ImmutableArray<byte> IV { get; }
    }
}
