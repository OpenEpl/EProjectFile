using QIQI.EProjectFile.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace QIQI.EProjectFile
{
    public class ClassPublicityInfo
    {
        public int Class { get; set; }
        public int Flags { get; set; }
        public bool Public { get => (Flags & 0x1) != 0; set => Flags = (Flags & ~0x1) | (value ? 0x1 : 0); }
        public bool Hidden { get => (Flags & 0x2) != 0; set => Flags = (Flags & ~0x2) | (value ? 0x2 : 0); }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}
