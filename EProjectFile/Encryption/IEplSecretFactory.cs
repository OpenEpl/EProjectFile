using System;
using System.Collections.Generic;
using System.Text;

namespace QIQI.EProjectFile.Encryption
{
    public interface IEplSecretFactory<out T> where T : EplSecret
    {
        public T Create(byte[] key);
        public T Create(string key);
    }
}
