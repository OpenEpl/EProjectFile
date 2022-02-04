using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QIQI.EProjectFile
{
    public class IdToNameMap
    {
        private static Regex debugCommentMatchRegex = new Regex(@"^_-@[MS]<([_A-Za-z\u0080-\uFFFF][_0-9A-Za-z\u0080-\uFFFF]*)>$", RegexOptions.Compiled);
        public static string ParseDebugComment(string comment)
        {
            var matchItem = debugCommentMatchRegex.Match(comment);
            if (matchItem == null || !matchItem.Success)
            {
                return null;
            }
            return matchItem.Groups[1].Value;
        }

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
        public Dictionary<int, string> UserDefinedName { get; }
        public Dictionary<int, int> MethodIdToClassId { get; }
        public LibInfo.LibInfo[] LibDefinedName { get; }
        /// <summary>
        /// 不加载名称数据模式（私有） 
        /// </summary>
        private IdToNameMap()
        {
#if NETSTANDARD1_3_OR_GREATER
            LibDefinedName = Array.Empty<LibInfo.LibInfo>();
#else
            LibDefinedName = new LibInfo.LibInfo[0];
#endif
            UserDefinedName = new Dictionary<int, string>();
            MethodIdToClassId = new Dictionary<int, int>();
        }
        /// <summary>
        /// 只加载支持库信息模式
        /// </summary>
        /// <param name="lib">需要加载信息的支持库列表</param>
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
            MethodIdToClassId = new Dictionary<int, int>();
        }

        /// <summary>
        /// <paramref name="codeSection"/>或<paramref name="resourceSection"/>改变时必须重新创建IDToNameMap以便更新数据（尽管部分数据可能自动更新）
        /// </summary>
        /// <param name="codeSection">程序段</param>
        /// <param name="resourceSection">资源段</param>
        public IdToNameMap(CodeSectionInfo codeSection, ResourceSectionInfo resourceSection) : this(codeSection, resourceSection, null)
        {

        }

        /// <summary>
        /// <paramref name="codeSection"/>或<paramref name="resourceSection"/>或<paramref name="losableSection"/>改变时必须重新创建IDToNameMap以便更新数据（尽管部分数据可能自动更新）
        /// </summary>
        /// <param name="codeSection">程序段</param>
        /// <param name="resourceSection">资源段</param>
        /// <param name="losableSection">可丢失程序段</param>
        public IdToNameMap(CodeSectionInfo codeSection, ResourceSectionInfo resourceSection, LosableSectionInfo losableSection) : this(codeSection?.Libraries)
        {
            if (codeSection != null)
            {
                foreach (var method in codeSection.Methods)
                {
                    if (string.IsNullOrEmpty(method.Name))
                    {
                        var symbol = ParseDebugComment(method.Comment);
                        if (symbol != null)
                        {
                            UserDefinedName.Add(method.Id, symbol);
                        }
                    }
                    else
                    {
                        UserDefinedName.Add(method.Id, method.Name);
                    }
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
                    foreach (var item in classInfo.Method)
                    {
                        MethodIdToClassId[item] = classInfo.Id;
                    }
                    if (string.IsNullOrEmpty(classInfo.Name))
                    {
                        var symbol = ParseDebugComment(classInfo.Comment);
                        if (symbol != null)
                        {
                            UserDefinedName.Add(classInfo.Id, symbol);
                        }
                    }
                    else
                    {
                        UserDefinedName.Add(classInfo.Id, classInfo.Name);
                    }
                    Array.ForEach(classInfo.Variables, x => UserDefinedName.Add(x.Id, x.Name));
                }
                foreach (var structInfo in codeSection.Structs)
                {
                    UserDefinedName.Add(structInfo.Id, structInfo.Name);
                    Array.ForEach(structInfo.Member, x => UserDefinedName.Add(x.Id, x.Name));
                }
                Array.ForEach(codeSection.GlobalVariables, x => UserDefinedName.Add(x.Id, x.Name));
            }
            if (resourceSection != null)
            {
                Array.ForEach(resourceSection.Constants, x => UserDefinedName.Add(x.Id, x.Name));
                foreach (var formInfo in resourceSection.Forms)
                {
                    UserDefinedName.Add(formInfo.Id, formInfo.Name);
                    Array.ForEach(formInfo.Elements, x => UserDefinedName.Add(x.Id, x.Name));
                }
            }
            if (losableSection != null)
            {
                foreach (var x in losableSection.RemovedDefinedItem)
                {
                    // 在删除、撤销等操作下，有效ID可能被记录到可丢失程序段，这些信息应当丢弃
                    if (!UserDefinedName.ContainsKey(x.Id))
                    {
                        UserDefinedName.Add(x.Id, x.Name);
                    }
                }
            }
            if (codeSection.MainMethod != 0) 
            {
                UserDefinedName[codeSection.MainMethod] = "_启动子程序";
            }

            // 处理无名对象
            var needToRemove = new List<int>();
            foreach (var item in UserDefinedName)
                if (string.IsNullOrEmpty(item.Value)) 
                    needToRemove.Add(item.Key);
            needToRemove.ForEach(x => UserDefinedName.Remove(x));
        }

        /// <summary>
        /// 首选构造方法
        /// </summary>
        /// <param name="source">源码文件</param>
        public IdToNameMap(EProjectFile source) : this(source.Code, source.Resource, source.LosableSection)
        {

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
            if (UserDefinedName.TryGetValue(id, out var result))
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
        public string GetLibTypeEventName(int typeId, int id)
        {
            EplSystemId.DecomposeLibDataTypeId(typeId, out var lib, out var type);
            return GetLibTypeEventName(lib, type, id);
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
        public string GetLibTypeMemberName(int typeId, int id)
        {
            EplSystemId.DecomposeLibDataTypeId(typeId, out var lib, out var type);
            return GetLibTypeMemberName(lib, type, id);
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
