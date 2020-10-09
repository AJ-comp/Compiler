using System.Collections.Generic;

namespace Parse.FrontEnd.Tokenize
{
    public class IndexMap : List<int>
    {
        public Range GetParsingRange()
        {
            List<int> result = new List<int>();

            foreach (var item in this)
            {
                if (item >= 0) result.Add(item);
            }

            return (result.Count > 0) ? new Range(0, result.Count) : new Range(-1, 0);
        }
    }
}
