using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Tokenize
{
    public class TokenPatternInfo
    {
        public int Key { get; }
        public Terminal Terminal { get; private set; }
        public string Pattern => Terminal.RegexExpression;
        public string OriginalPattern => Terminal.Value;
        public bool IsWord => Terminal.IsWordPattern;
        public bool Operator => Terminal.IsOper;


        public static TokenPatternInfo NotDefinedToken { get => new TokenPatternInfo(0, new NotDefined()); }

        public TokenPatternInfo(int key, Terminal terminal)
        {
            this.Key = key;
            this.Terminal = terminal;
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

        public override string ToString() => string.Format("{0}, {1}, {2}, {3}", this.Key, this.Pattern, this.IsWord.ToString().ToLower(), this.Operator.ToString().ToLower());
    }
}
