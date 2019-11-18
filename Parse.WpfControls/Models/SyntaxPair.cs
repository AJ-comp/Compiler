using System.Collections.Generic;

namespace Parse.WpfControls.Models
{
    public class SyntaxPair
    {
        public string StartString { get; }
        public string EndString { get; }

        public SyntaxPair(string startString, string endString)
        {
            this.StartString = startString;
            this.EndString = endString;
        }

        public override string ToString() => string.Format("{0},{1}", this.StartString, this.EndString);
    }


    public class SyntaxPairCollection : List<SyntaxPair>
    {
        public string GetEndString(string startString)
        {
            string result = string.Empty;

            foreach(var item in this)
            {
                if (item.StartString == startString)
                {
                    result = item.EndString;
                    break;
                }
            }

            return result;
        }
    }
}
