namespace Parse.Tokenize
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


        public override string ToString() => string.Format("{0}, {1}, {2}, {3}", this.Key, this.Pattern, this.CanDerived.ToString().ToLower(), this.Operator.ToString().ToLower());
    }
}
