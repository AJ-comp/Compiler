using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.InterLanguages
{
    public class IRBlock : List<IRUnit>, IRUnit
    {
        private string _comment;

        public string Label { get; }
        public string Comment => (_comment.Length > 0) ? "; " + _comment : string.Empty;

        public IRBlock()
        {
        }

        public IRBlock(string label, string comment = "")
        {
            Label = label;
            _comment = comment;
        }

        public string ToFormatString()
        {
            string result = Comment + Environment.NewLine;
            result += Label + Environment.NewLine;

            foreach (var item in this)
                result += item + Environment.NewLine;

            return result;
        }
    }
}
