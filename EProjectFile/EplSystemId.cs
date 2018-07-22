using System;

namespace QIQI.EProjectFile
{
    public class EplSystemId
    {
        /// <summary>
        /// Not a Variable
        /// </summary>
        /// <seealso cref="AccessArrayExpression"/>
        /// <seealso cref="AccessMemberExpression"/>
        internal const int Id_NaV = 0x0500FFFE;

        public const int Type_Method = 0x04000000;
        public const int Type_Global = 0x05000000;
        /// <summary>
        /// 普通程序集（非窗口程序集、类模块）
        /// </summary>
        public const int Type_StaticClass = 0x09000000;
        public const int Type_Dll = 0x0A000000;
        /// <summary>
        /// 程序集变量 或 类模块成员
        /// </summary>
        public const int Type_ClassMember = 0x15000000;
        public const int Type_Control = 0x16000000;
        public const int Type_Constant = 0x18000000;
        /// <summary>
        /// 窗口程序集
        /// </summary>
        public const int Type_FormClass = 0x19000000;
        /// <summary>
        /// 局部变量 或 子程序参数（非DLL命令）
        /// </summary>
        public const int Type_Local = 0x25000000;
        public const int Type_ImageResource = 0x28000000;
        public const int Type_StructMember = 0x35000000;
        public const int Type_SoundResource = 0x38000000;
        public const int Type_Struct = 0x41000000;
        public const int Type_DllParameter = 0x45000000;
        public const int Type_Class = 0x49000000;
        public const int Mask_Num = 0x00FFFFFF;
        public const int Mask_Type = unchecked((int)0xFF000000);

        /// <summary>
        /// 只用于用户定义Id
        /// </summary>
        /// <param name="id">欲获取类型的Id</param>
        /// <returns>指定Id所属类型，参考EplSystemId.Type_*</returns>
        public static int GetType(int id) => id & Mask_Type;
        
        /// <summary>
        /// 无类型（如 某子程序无返回值）
        /// </summary>
        public const int DataType_Void = unchecked((int)0x00000000);
        /// <summary>
        /// 通用型
        /// </summary>
        public const int DataType_Any = unchecked((int)0x80000000);
        public const int DataType_Byte = unchecked((int)0x80000101);
        public const int DataType_Short = unchecked((int)0x80000201);
        public const int DataType_Int = unchecked((int)0x80000301);
        public const int DataType_Long = unchecked((int)0x80000401);
        public const int DataType_Float = unchecked((int)0x80000501);
        public const int DataType_Double = unchecked((int)0x80000601);
        public const int DataType_Bool = unchecked((int)0x80000002);
        public const int DataType_DateTime = unchecked((int)0x80000003);
        public const int DataType_String = unchecked((int)0x80000004);
        public const int DataType_Bin = unchecked((int)0x80000005);
        public const int DataType_MethodPtr = unchecked((int)0x80000006);

        public static int MakeSureIsSpecifiedType(int id, params int[] type) => Array.IndexOf(type, GetType(id)) >= 0 ? id : throw new Exception("不是指定类型的Id");

        public static bool IsLibDataType(int id) => (id & 0xF0000000) == 0 && id != DataType_Void;

        /// <summary>
        /// 合成库类型Id
        /// </summary>
        /// <param name="lib">索引从0开始（CStyle）</param>
        /// <param name="type">索引从0开始（CStyle）</param>
        /// <returns></returns>
        public static int MakeLibDataTypeId(short lib, short type) => ((int)(lib + 1) << 16) | (int)(type + 1);
        /// <summary>
        /// 分解库类型Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lib">索引从0开始（CStyle）</param>
        /// <param name="type">索引从0开始（CStyle）</param>
        public static void DecomposeLibDataTypeId(int id, out short lib, out short type)
        {
            if (!IsLibDataType(id)) throw new Exception("DecomposeLibDataTypeId只能处理库类型Id");
            unchecked
            {
                lib = (short)(id >> 16);
                lib--;
                type = (short)(id);
                type--;
            }
        }
    }
}
