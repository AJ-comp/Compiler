namespace Parse
{
    public class ScopeSyntax
    {
        public string StartSyntax { get; } = string.Empty;
        public string EndSyntax { get; } = string.Empty;

        public bool IsEmpty { get => !(this.StartSyntax.Length > 0 && this.EndSyntax.Length > 0); }

        public ScopeSyntax()
        {
        }

        public ScopeSyntax(string startString, string endString)
        {
            this.StartSyntax = startString;
            this.EndSyntax = endString;
        }

        public override string ToString() => string.Format("{0},{1}", this.StartSyntax, this.EndSyntax);
    }
}
