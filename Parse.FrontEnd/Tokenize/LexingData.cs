namespace Parse.FrontEnd.Tokenize
{
    public class LexingData
    {
        public TokenStorage TokenStorage { get; }
        public TokenizeImpactRanges RangeToParse { get; }

        public LexingData(TokenStorage tokenStorage, TokenizeImpactRanges impactRanges)
        {
            TokenStorage = tokenStorage;
            RangeToParse = impactRanges;
        }
    }
}
