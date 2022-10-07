
using QIQI.EProjectFile.Context;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace QIQI.EProjectFile.Sections
{
    public class EndOfFileSection : ISection
    {
        private class KeyImpl : ISectionKey<EndOfFileSection>
        {
            public string SectionName => "";
            public int SectionKey => 0x07007319;
            public bool IsOptional => false;

            public EndOfFileSection Parse(BlockParserContext context)
            {
                if (context.DataLength > 0)
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

        public byte[] ToBytes(BlockByteifierContext context)
        {
            return Array.Empty<byte>();
        }
    }
}
