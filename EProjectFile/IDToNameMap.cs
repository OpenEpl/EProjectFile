using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace QIQI.EProjectFile
{
    public class IdToNameMap
    {
        public static readonly Dictionary<int, string> SystemDataTypeName = new Dictionary<int, string> {
            { EplSystemId.DataType_Bin, "字节集" },
            { EplSystemId.DataType_Bool, "逻辑型" },
            { EplSystemId.DataType_Byte, "字节型" },
            { EplSystemId.DataType_DateTime, "日期时间型" },
            { EplSystemId.DataType_Double, "双精度小数型" },
            { EplSystemId.DataType_Float, "小数型" },
            { EplSystemId.DataType_Int, "整数型" },
            { EplSystemId.DataType_Long, "长整数型" },
            { EplSystemId.DataType_MethodPtr, "子程序指针" },
            { EplSystemId.DataType_Short, "短整数型" },
            { EplSystemId.DataType_String, "文本型" },
            { EplSystemId.DataType_Void, "" }
        };
        public static readonly IdToNameMap Empty = new IdToNameMap();
        private readonly Dictionary<int, string> userDefinedName;
        private readonly LibInfo.LibInfo[] libDefinedName;
        /// <summary>
        /// 不加载名称数据模式（私有） 
        /// </summary>
        private IdToNameMap()
        {
            libDefinedName = new LibInfo.LibInfo[0];
            userDefinedName = new Dictionary<int, string>();
        }
        /// <summary>
        /// 只加载支持库信息模式
        /// </summary>
        /// <param name="lib">需要加载信息的支持库列表</param>
        public IdToNameMap(LibraryRefInfo[] lib)
        {
            libDefinedName = lib.Select(x =>
            {
                try
                {
                    return LibInfo.LibInfo.Load(x);
                }
                catch (Exception)
                {
                    return null;
                }
            }).ToArray();
            userDefinedName = new Dictionary<int, string>();
        }
        /// <summary>
        /// <paramref name="codeSection"/>或<paramref name="resourceSection"/>改变时必须重新创建IDToNameMap以便更新数据（尽管部分数据可能自动更新）
        /// </summary>
        /// <param name="codeSection">代码段</param>
        /// <param name="resourceSection">资源段</param>
        public IdToNameMap(CodeSectionInfo codeSection, ResourceSectionInfo resourceSection)
        {
            libDefinedName = codeSection.Libraries.Select(x =>
            {
                try
                {
                    return LibInfo.LibInfo.Load(x);
                }
                catch (Exception)
                {
                    return null;
                }
            }).ToArray();
            userDefinedName = new Dictionary<int, string>();
            foreach (var method in codeSection.Methods)
            {
                userDefinedName.Add(method.Id, method.Name);
                Array.ForEach(method.Parameters, x => userDefinedName.Add(x.Id, x.Name));
                Array.ForEach(method.Variables, x => userDefinedName.Add(x.Id, x.Name));
            }
            foreach (var dll in codeSection.DllDeclares)
            {
                userDefinedName.Add(dll.Id, dll.Name);
                Array.ForEach(dll.Parameters, x => userDefinedName.Add(x.Id, x.Name));
            }
            foreach (var classInfo in codeSection.Classes)
            {
                userDefinedName.Add(classInfo.Id, classInfo.Name);
                Array.ForEach(classInfo.Variables, x => userDefinedName.Add(x.Id, x.Name));
            }
            foreach (var structInfo in codeSection.Structs)
            {
                userDefinedName.Add(structInfo.Id, structInfo.Name);
                Array.ForEach(structInfo.Member, x => userDefinedName.Add(x.Id, x.Name));
            }
            Array.ForEach(codeSection.GlobalVariables, x => userDefinedName.Add(x.Id, x.Name));
            Array.ForEach(resourceSection.Constants, x => userDefinedName.Add(x.Id, x.Name));
            foreach (var formInfo in resourceSection.Forms)
            {
                userDefinedName.Add(formInfo.Id, formInfo.Name);
                Array.ForEach(formInfo.Elements, x => userDefinedName.Add(x.Id, x.Name));
            }

            var needToRemove = new List<int>();
            foreach (var item in userDefinedName)
                if (string.IsNullOrEmpty(item.Value)) 
                    needToRemove.Add(item.Key);
            needToRemove.ForEach(x => userDefinedName.Remove(x));
        }

        private static readonly Dictionary<int, string> IdTypeName = new Dictionary<int, string> {
            { EplSystemId.Type_Method, "Sub" },
            { EplSystemId.Type_Global, "Global" },
            { EplSystemId.Type_StaticClass, "Mod" },
            { EplSystemId.Type_Dll, "Dll" },
            { EplSystemId.Type_ClassMember, "Mem" },
            { EplSystemId.Type_Control, "Control" },
            { EplSystemId.Type_Constant, "Const" },
            { EplSystemId.Type_FormClass, "FormCls" },
            { EplSystemId.Type_Local, "Local" },
            { EplSystemId.Type_ImageResource, "Img" },
            { EplSystemId.Type_SoundResource, "Sound" },
            { EplSystemId.Type_StructMember, "StructMem" },
            { EplSystemId.Type_Struct, "Struct" },
            { EplSystemId.Type_DllParameter, "DllParam" },
            { EplSystemId.Type_Class, "Cls" }
        };

        public string GetUserDefinedName(int id)
        {
            if (userDefinedName.TryGetValue(id, out var result))
            {
                return result;
            }
            else if (IdTypeName.TryGetValue(EplSystemId.GetType(id), out result))
            {
                return $"_{result}_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
            }
            else
            {
                return $"_User_0x{id.ToString("X8")}";
            }
        }
        public string GetLibCmdName(int lib, int id)
        {
            try
            {
                return libDefinedName[lib].Cmd[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Cmd{id}";
            }
        }
        public string GetLibTypeName(int lib, int id)
        {
            try
            {
                return libDefinedName[lib].DataType[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Type{id}";
            }
        }
        public string GetLibConstantName(int lib, int id)
        {
            try
            {
                return libDefinedName[lib].Constant[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Const{id}";
            }
        }
        public string GetLibTypeEventName(int typeId, int id)
        {
            EplSystemId.DecomposeLibDataTypeId(typeId, out var lib, out var type);
            return GetLibTypeEventName(lib, type, id);
        }
        public string GetLibTypeEventName(int lib, int typeId, int id)
        {
            try
            {
                return libDefinedName[lib].DataType[typeId].Evnet[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Type{typeId}Event{id}";
            }
        }
        public string GetLibTypePropertyName(int typeId, int id)
        {
            EplSystemId.DecomposeLibDataTypeId(typeId, out var lib, out var type);
            return GetLibTypePropertyName(lib, type, id);
        }
        public string GetLibTypePropertyName(int lib, int typeId, int id)
        {
            try
            {
                return libDefinedName[lib].DataType[typeId].Property[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Type{typeId}Prop{id}";
            }
        }
        public string GetLibTypeMemberName(int typeId, int id)
        {
            EplSystemId.DecomposeLibDataTypeId(typeId, out var lib, out var type);
            return GetLibTypeMemberName(lib, type, id);
        }
        public string GetLibTypeMemberName(int lib, int typeId, int id)
        {
            try
            {
                return libDefinedName[lib].DataType[typeId].Member[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Type{typeId}Mem{id}";
            }
        }
        public string GetDataTypeName(int id)
        {
            if (SystemDataTypeName.TryGetValue(id, out var result))
            {
                return result;
            }
            else if (EplSystemId.IsLibDataType(id))
            {
                EplSystemId.DecomposeLibDataTypeId(id, out var lib, out var type);
                return GetLibTypeName(lib, type);
            }
            else
            {
                return GetUserDefinedName(id);
            }
        }
    }
}
