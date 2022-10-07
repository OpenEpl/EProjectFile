using QIQI.EProjectFile.Context;
using QIQI.EProjectFile.Internal;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace QIQI.EProjectFile.EditorTabInfo
{
    public interface IEditorTabInfoKey<out TEditorTabInfo> where TEditorTabInfo : IEditorTabInfo
    {
        public byte TypeId { get; }
        TEditorTabInfo Parse(BlockParserContext context);
    }
    [JsonConverter(typeof(EditorTabInfoJsonConverter))]
    public interface IEditorTabInfo
    {
        public byte TypeId { get; }
        public void WriteTo(BinaryWriter writer, Encoding encoding);
    }
}
