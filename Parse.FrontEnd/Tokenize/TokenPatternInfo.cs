using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Tokenize
{
    public class TokenPatternInfo
    {
        public int Key { get; }
        public object OptionData { get; }
        public string Pattern
        {
            get
            {
                if (this.Operator)
                {
                    string convertString = string.Empty;
                    foreach (var c in this.OriginalPattern)
                        convertString += "\\" + c;

                    return convertString;
                }
                else
                {
                    return (this.CanDerived) ? this.OriginalPattern : "\\b" + this.OriginalPattern + "\\b";
                }
            }
        }

        public static TokenPatternInfo NotDefinedToken { get => new TokenPatternInfo(0, string.Empty); }

        public string OriginalPattern { get; }
        public bool CanDerived { get; }
        public bool Operator { get; }

        public TokenPatternInfo(int key, string pattern, object optionData = null, bool bCanDerived = false, bool bOperator = false)
        {
            this.Key = key;
            this.OptionData = optionData;
            this.OriginalPattern = pattern;
            this.CanDerived = bCanDerived;
            this.Operator = bOperator;
        }


        public bool Equals(TokenPatternInfo other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return (this.GetHashCode() == other.GetHashCode());
        }

        public override int GetHashCode() => (int)this.Key;

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is TokenPatternInfo)
            {
                TokenPatternInfo right = obj as TokenPatternInfo;

                result = (this.GetHashCode() == right.GetHashCode());
            }

            return result;
        }

        public override string ToString() => string.Format("{0}, {1}, {2}, {3}", this.Key, this.Pattern, this.CanDerived.ToString().ToLower(), this.Operator.ToString().ToLower());



        private Terminal _terminal;
    }
}
