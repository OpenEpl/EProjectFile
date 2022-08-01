using QIQI.EProjectFile.Internal;
using System.IO;
using System.Text;

namespace QIQI.EProjectFile
{
    public abstract class FormElementInfo : IHasId
    {
        public int Id { get; private set; }

        public int DataType { get; set; }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public bool Disable { get; set; }
        public static FormElementInfo[] ReadFormElements(BinaryReader r, Encoding encoding)
        {
            return r.ReadBlocksWithIdAndOffest((reader, id, length) =>
            {
                var dataType = reader.ReadInt32();
                FormElementInfo elem;
                if (dataType == 65539)
                {
                    elem = FormMenuInfo.ReadWithoutDataType(r, encoding, length - 4);
                }
                else
                {
                    elem = FormControlInfo.ReadWithoutDataType(r, encoding, length - 4);
                }
                elem.Id = id;
                elem.DataType = dataType;
                return elem;
            });
        }
        public static void WriteFormElements(BinaryWriter w, Encoding encoding, FormElementInfo[] formElements)
        {
            w.WriteBlocksWithIdAndOffest(
                encoding,
                formElements, 
                (writer, elem) =>
                {
                    elem.WriteWithoutId(writer, encoding);
                });
        }

        protected abstract void WriteWithoutId(BinaryWriter writer, Encoding encoding);
    }
}
