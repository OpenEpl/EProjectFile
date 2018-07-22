namespace QIQI.EProjectFile.Statements
{
    /// <summary>
    /// 循环语句块 基类
    /// </summary>
    public abstract class LoopStatement : Statement
    {
        public StatementBlock Block { get; set; }
        /// <summary>
        /// <see cref="UnexaminedCode"/>不为null时，其他循环参数应为null
        /// </summary>
        public string UnexaminedCode { get; set; }
        public string CommentOnStart { get; set; }
        public string CommentOnEnd { get; set; }
        public bool MaskOnStart { get; set; }
        public bool MaskOnEnd { get; set; }
    }
}
