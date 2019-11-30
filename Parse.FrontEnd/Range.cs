namespace Parse
{
    public class Range
    {
        public int StartIndex { get; }
        public int Count { get; }
        public int EndIndex { get; }

        public Range(int startIndex, int count)
        {
            this.StartIndex = startIndex;
            this.Count = count;
            this.EndIndex = this.StartIndex + this.Count - 1;
        }

        public override string ToString() => string.Format("{0}~{1}:{2}", this.StartIndex, this.EndIndex, this.Count);
    }
}
