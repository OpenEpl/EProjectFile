namespace QIQI.EProjectFile.Expressions
{
    public abstract class In0x38Expression : Expression
    {
        internal abstract void WriteTo(MethodCodeDataWriterArgs a, bool need0x1DAnd0x37);
        internal override void WriteTo(MethodCodeDataWriterArgs a)
        {
            WriteTo(a, true);
        }
    }
}
