namespace Parse.RegularGrammar
{
    public enum BridgeType { Concatenation, Alternation };

    public interface IBridgeable
    {
        BridgeType BridgeType { get; }
    }
}
