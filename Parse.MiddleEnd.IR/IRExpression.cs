namespace Parse.MiddleEnd.IR
{
    public abstract class IRExpression
    {
        public string Name => GetType().Name;
    }
}
