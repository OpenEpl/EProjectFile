using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace QIQI.EProjectFile
{
    public class IdToNameMap
    {
        public static readonly IdToNameMap Empty = new IdToNameMap();
        private readonly Dictionary<int, string> UserDefinedName;
        private readonly LibInfo.LibInfo[] LibDefinedName;
        /// <summary>
        /// 不加载名称数据模式（私有）
        /// </summary>
        /// <param name="lib"></param>
        private IdToNameMap()
        {
            LibDefinedName = new LibInfo.LibInfo[0];
            UserDefinedName = new Dictionary<int, string>();
        }
        /// <summary>
        /// 只加载支持库信息模式
        /// </summary>
        /// <param name="lib"></param>
        public IdToNameMap(LibraryRefInfo[] lib)
        {
            LibDefinedName = lib.Select(x =>
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
            UserDefinedName = new Dictionary<int, string>();
        }
        /// <summary>
        /// CodeSectionInfo改变时必须重新创建IDToNameMap以便更新数据（尽管部分数据可能自动更新）
        /// </summary>
        /// <param name="codeSection"></param>
        public IdToNameMap(CodeSectionInfo codeSection, ResourceSectionInfo resourceSection)
        {
            LibDefinedName = codeSection.Libraries.Select(x =>
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
            UserDefinedName = new Dictionary<int, string>();
            foreach (var method in codeSection.Methods)
            {
                UserDefinedName.Add(method.Id, method.Name);
                Array.ForEach(method.Parameters, x => UserDefinedName.Add(x.Id, x.Name));
                Array.ForEach(method.Variables, x => UserDefinedName.Add(x.Id, x.Name));
            }
            foreach (var dll in codeSection.DllDeclares)
            {
                UserDefinedName.Add(dll.Id, dll.Name);
                Array.ForEach(dll.Parameters, x => UserDefinedName.Add(x.Id, x.Name));
            }
            foreach (var classInfo in codeSection.Classes)
            {
                UserDefinedName.Add(classInfo.Id, classInfo.Name);
                Array.ForEach(classInfo.Variables, x => UserDefinedName.Add(x.Id, x.Name));
            }
            foreach (var structInfo in codeSection.Structs)
            {
                UserDefinedName.Add(structInfo.Id, structInfo.Name);
                Array.ForEach(structInfo.Member, x => UserDefinedName.Add(x.Id, x.Name));
            }
            Array.ForEach(codeSection.GlobalVariables, x => UserDefinedName.Add(x.Id, x.Name));
            Array.ForEach(resourceSection.Constants, x => UserDefinedName.Add(x.Id, x.Name));
            foreach (var formInfo in resourceSection.Forms)
            {
                UserDefinedName.Add(formInfo.Id, formInfo.Name);
                Array.ForEach(formInfo.Elements, x => UserDefinedName.Add(x.Id, x.Name));
            }

            var needToRemove = new List<int>();
            foreach(var item in UserDefinedName) if(string.IsNullOrEmpty(item.Value))
                    needToRemove.Add(item.Key);
            needToRemove.ForEach(x => UserDefinedName.Remove(x));
        }
        public string GetUserDefinedName(int id)
        {
            
            if (UserDefinedName.TryGetValue(id, out var result))
                return result;
            else
                switch (EplSystemId.GetType(id))
                {
                    case EplSystemId.Type_Method:
                        return $"_Sub_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_Global:
                        return $"_Global_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_StaticClass:
                        return $"_Mod_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_Dll:
                        return $"_Dll_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_ClassMember:
                        return $"_Mem_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_Control:
                        return $"_Control_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_Constant:
                        return $"_Const_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_FormClass:
                        return $"_FormCls_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_Local:
                        return $"_Local_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_ImageResource:
                        return $"_Img_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_SoundResource:
                        return $"_Sound_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_StructMember:
                        return $"_StructMem_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_Struct:
                        return $"_Struct_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_DllParameter:
                        return $"_DllParam_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    case EplSystemId.Type_Class:
                        return $"_Cls_0x{(id & EplSystemId.Mask_Num).ToString("X6")}";
                    default:
                        return $"_User_0x{id.ToString("X8")}";
                }
        }
        public string GetLibCmdName(int lib,int id)
        {
            try
            {
                return LibDefinedName[lib].Cmd[id].Name;
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
                return LibDefinedName[lib].DataType[id].Name;
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
                return LibDefinedName[lib].Constant[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Const{id}";
            }
        }
        public string GetLibTypeEventName(int lib, int typeId, int id)
        {
            try
            {
                return LibDefinedName[lib].DataType[typeId].Evnet[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Type{typeId}Event{id}";
            }
        }
        public string GetLibTypePropertyName(int lib, int typeId, int id)
        {
            try
            {
                return LibDefinedName[lib].DataType[typeId].Property[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Type{typeId}Prop{id}";
            }
        }
        public string GetLibTypeMemberName(int lib, int typeId, int id)
        {
            try
            {
                return LibDefinedName[lib].DataType[typeId].Member[id].Name;
            }
            catch (Exception)
            {
                return $"_Lib{lib}Type{typeId}Mem{id}";
            }
        }
    }
}
