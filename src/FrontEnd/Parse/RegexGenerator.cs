namespace Parse
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
            string result = string.Empty;

            foreach (var c in data) result += "\\" + c;

            return result;
        }
    }
}
