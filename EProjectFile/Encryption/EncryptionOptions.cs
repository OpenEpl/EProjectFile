using System;
using System.Collections.Generic;
using System.Text;

namespace QIQI.EProjectFile.Encryption
{
    public abstract class EplEncryptionOptions
    {
        public sealed class EStd : EplEncryptionOptions, IEquatable<EStd>
        {
            public EplSecret.EStd Password;

            public override bool Equals(object obj)
            {
                return Equals(obj as EStd);
            }

            public bool Equals(EStd other)
            {
                return !(other is null) &&
                       EqualityComparer<EplSecret.EStd>.Default.Equals(Password, other.Password);
            }

            public override int GetHashCode()
            {
                return -1081153288 + EqualityComparer<EplSecret.EStd>.Default.GetHashCode(Password);
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
        public sealed class EC : EplEncryptionOptions, IEquatable<EC>
        {
            public EplSecret.EC Password;
            public string PasswordHint;

            public override bool Equals(object obj)
            {
                return Equals(obj as EC);
            }

            public bool Equals(EC other)
            {
                return !(other is null) &&
                       EqualityComparer<EplSecret.EC>.Default.Equals(Password, other.Password) &&
                       PasswordHint == other.PasswordHint;
            }

            public override int GetHashCode()
            {
                int hashCode = 1281113049;
                hashCode = hashCode * -1521134295 + EqualityComparer<EplSecret.EC>.Default.GetHashCode(Password);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PasswordHint);
                return hashCode;
            }

            public static bool operator ==(EC left, EC right)
            {
                return EqualityComparer<EC>.Default.Equals(left, right);
            }

            public static bool operator !=(EC left, EC right)
            {
                return !(left == right);
            }
        }
    }
}
