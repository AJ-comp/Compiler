using System.Text.RegularExpressions;

namespace Janglim.WpfControls.Utilities
{
    static class RegexUtility
    {
        public static int EndIndex(this Match match) => match.Index + match.Length - 1;
    }
}
