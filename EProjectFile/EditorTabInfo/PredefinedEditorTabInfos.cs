using System.Collections.Generic;
using System.Linq;

namespace QIQI.EProjectFile.EditorTabInfo
{
    public class PredefinedEditorTabInfos
    {
        public static Dictionary<byte, IEditorTabInfoKey<IEditorTabInfo>> Keys { get; } = new IEditorTabInfoKey<IEditorTabInfo>[]{
            ClassEditorTabInfo.Key,
            StructEditorTabInfo.Key,
            GlobalVariableEditorTabInfo.Key,
            DllDeclareEditorTabInfo.Key,
            FormDesignerTabInfo.Key,
            ConstantEditorTabInfo.Key,
            ImageResourceEditorTabInfo.Key,
            SoundResourceEditorTabInfo.Key,
        }.ToDictionary(x => x.TypeId);
    }
}
