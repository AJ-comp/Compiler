namespace Parse.FrontEnd.InterLanguages
{
    public class IROptions
    {
        public string Label { get; }
        public string Comment { get; }

        public IROptions(string label)
        {
            Label = label;
        }

        public IROptions(string label, string comment) : this(label)
        {
            Comment = comment;
        }
    }
}
