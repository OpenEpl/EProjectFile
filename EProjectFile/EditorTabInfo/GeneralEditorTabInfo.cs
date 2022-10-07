using QIQI.EProjectFile.Internal;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile.EditorTabInfo
{
    public class GeneralEditorTabInfo : IEditorTabInfo
    {
        [JsonConstructor]
        public GeneralEditorTabInfo(byte type, byte[] data)
        {
            TypeId = type;
            Data = data;
        }

        public byte TypeId { get; }

        [JsonConverter(typeof(ByteArrayHexConverter))]
        public byte[] Data { get; set; }

        public void WriteTo(BinaryWriter writer, Encoding encoding)
        {
            if (Data is null)
            {
                writer.Write(1);
                writer.Write(TypeId);
                return;
            }
            writer.Write(1 + Data.Length);
            writer.Write(TypeId);
            writer.Write(Data);
        }
    }
}
