namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Selects the initial strictness of Script or expression input. A use-strict directive,
    /// a containing strict function, or class code can still make nested code strict.
    /// Module input is always strict. Browser compatibility is a separate option.
    /// <para><see href="../../../../docs/ko/javascript-non-strict.html">비엄격 모드와 브라우저 호환 문법</see></para>
    /// </summary>
    public enum JavaScriptMode
    {
        Strict,
        NonStrict
    }
}
