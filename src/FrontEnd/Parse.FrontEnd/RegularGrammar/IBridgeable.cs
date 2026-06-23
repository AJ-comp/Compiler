namespace Parse.FrontEnd.RegularGrammar
{
    public enum BridgeType { Concatenation, Alternation };

    public interface IBridgeable
    {
        BridgeType BridgeType { get; }
    }
}
