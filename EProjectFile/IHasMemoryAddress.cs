using System;
using System.Collections.Generic;
using System.Text;

namespace QIQI.EProjectFile
{
    public interface IHasMemoryAddress
    {
        /// <summary>
        /// 最后一次保存时易语言 IDE 储存相关结构使用的内存地址
        /// 在用于编辑文件的场景下，意义不大
        /// </summary>
        int MemoryAddress { get; set; }
    }
}
