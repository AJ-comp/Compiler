using System.Text.RegularExpressions;

namespace Janglim
{
    public class RegexGenerator
    {
        public static string GetWordRegex(string data)
        {
            if (data[0] == '#' && char.IsLetter(data[1]))
                return string.Format("#\\b{0}\\b", data.Substring(1));

            return string.Format("\\b{0}\\b", data);
        }

        public static string GetOperatorRegex(string data)
        {
            // Escape only regex metacharacters. A backslash before EVERY char (the old way)
            // turned letters and digits into regex escapes: \i is an invalid escape and \2 is
            // a backreference, so operator values containing letters or digits (e.g. "id", "2")
            // produced broken or silently-dead patterns.
            return Regex.Escape(data);
        }
    }
}
