using System.IO;

namespace QIQI.EProjectFile
{
    public abstract class FormElementInfo : IHasId
    {
        private int id;
        public int Id => id;

        public int DataType;
        public string Name;
        public bool Visible;
        public bool Disable;
        public static FormElementInfo[] ReadFormElements(BinaryReader r)
        {
            return r.ReadBlocksWithIdAndOffest((reader, id, length) =>
            {
                var dataType = reader.ReadInt32();
                FormElementInfo elem;
                if (dataType == 65539)
                {
                    elem = FormMenuInfo.ReadWithoutDataType(r, length - 4);
                }
                else
                {
                    elem = FormControlInfo.ReadWithoutDataType(r, length - 4);
                }
                elem.id = id;
                elem.DataType = dataType;
                return elem;
            });
        }
        public static void WriteFormElements(BinaryWriter w, FormElementInfo[] formElements)
        {
            w.WriteBlocksWithIdAndOffest(formElements, (writer, elem) =>
            {
                elem.WriteWithoutId(writer);
            });
        }

        protected abstract void WriteWithoutId(BinaryWriter writer);
    }
}
