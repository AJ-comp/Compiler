using System;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Checks Unicode property escape names against ECMAScript's explicit whitelist
    /// and the pinned Unicode 17 aliases, without using the host regex engine.
    /// See <see href="../../../../docs/ko/javascript-regexp.html#properties">Unicode 속성 이름 검사</see>.
    /// </summary>
    internal static partial class JavaScriptRegExpProperties
    {
        internal static bool IsValid(string text, bool unicodeSets, bool negated,
            out bool mayContainStrings)
        {
            mayContainStrings = false;
            if (string.IsNullOrEmpty(text)) return false;
            int equal = text.IndexOf('=');
            if (equal >= 0)
            {
                string property = text.Substring(0, equal);
                string value = text.Substring(equal + 1);
                if (property == "General_Category" || property == "gc")
                    return Contains(GeneralCategories, value);
                if (property == "Script" || property == "sc" ||
                    property == "Script_Extensions" || property == "scx")
                    return Contains(Scripts, value);
                return false;
            }
            if (Contains(GeneralCategories, text) || Contains(BinaryProperties, text))
                return true;
            if (!Contains(StringProperties, text)) return false;
            mayContainStrings = true;
            return unicodeSets && !negated;
        }

        // ECMAScript property escapes use exact spelling. UCD loose matching
        // (case folding, ignored spaces and underscores) would accept invalid JS.
        private static bool Contains(string[] values, string value) =>
            Array.BinarySearch(values, value, StringComparer.Ordinal) >= 0;
    }
}
