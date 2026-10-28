namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Selects the syntax root used by <see cref="JavaScriptGrammar"/>. Script accepts
    /// ordinary declarations and statements, Module also accepts module declarations
    /// and applies module-specific checks, and Expression accepts one expression.
    /// <para><see href="../../../../docs/ko/javascript-modules.html#api">JavaScript 입력 종류와 사용 예제</see></para>
    /// </summary>
    public enum JavaScriptInputKind
    {
        /// <summary>Ordinary Script input. This is the default kind.</summary>
        Script,
        /// <summary>Module input, including import/export and module-specific strictness.</summary>
        Module,
        /// <summary>A single expression in a Script strictness context.</summary>
        Expression
    }
}
