namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptCustomTypeInfo
    {
        string TypeName { get; }
    }
}