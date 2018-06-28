namespace QIQI.EProjectFile
{
    public class EplSystemId
    {
        /// <summary>
        /// Not a Variable
        /// </summary>
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

        public static int GetType(int id) => id & Mask_Type;
        
        public const int DataType_Void = unchecked((int)0x00000000);
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
    }
}
