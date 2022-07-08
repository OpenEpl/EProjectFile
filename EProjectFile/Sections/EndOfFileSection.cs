
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace QIQI.EProjectFile.Sections
{
    public class EndOfFileSection : ISection
    {
        private class KeyImpl : ISectionKey<EndOfFileSection>
        {
            public string SectionName => "";
            public int SectionKey => 0x07007319;
            public bool IsOptional => false;

            public EndOfFileSection Parse(byte[] data, Encoding encoding, bool cryptEC)
            {
                if (data != null && data.Length > 0)
                {
                    throw new Exception("EndOfFileSection should be empty");
                }
                return Instance;
            }
        }

        public static readonly ISectionKey<EndOfFileSection> Key = new KeyImpl();

        public static EndOfFileSection Instance { get; } = new EndOfFileSection();
        private EndOfFileSection()
        {
        }

        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public byte[] ToBytes(Encoding encoding)
        {
            return Array.Empty<byte>();
        }
    }
}
