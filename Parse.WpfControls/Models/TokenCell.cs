namespace Parse.WpfControls.Models
{
    public enum RecognitionWay { Front, Back };

    public class TokenCell
    {
        public int StartIndex { get; internal set; }
        public int EndIndex { get => this.StartIndex + this.Data.Length - 1; }
        public string Data { get; } = string.Empty;
        public TokenPatternInfo PatternInfo { get; }

        public TokenCell(int startIndex, string data, TokenPatternInfo patternInfo)
        {
            this.StartIndex = startIndex;
            this.Data = data;

            this.PatternInfo = patternInfo;
        }

        public bool Contains(int caretIndex, RecognitionWay recognitionWay)
        {
            return (recognitionWay == RecognitionWay.Back) ?
                (this.StartIndex < caretIndex && caretIndex <= this.EndIndex + 1) : (this.StartIndex <= caretIndex && caretIndex <= this.EndIndex);
        }

        public bool MoreRange(int startingPos, int endingPos) => (startingPos <= this.StartIndex && this.EndIndex < endingPos);
        public bool SubRange(int startingPos, int endingPos) => (startingPos < this.StartIndex && this.EndIndex >= endingPos);

        /// <summary>
        /// This function returns a merged string if the caret index passed through the condition.
        /// </summary>
        /// <param name="caretIndex">The caret index.</param>
        /// <param name="addString">If passed through the condition then would merge this string. </param>
        /// <param name="recognitionWay">This argument means how to recognize the caret index.</param>
        /// <returns>The merged string.</returns>
        public string MergeString(int caretIndex, string addString, RecognitionWay recognitionWay)
        {
            if (addString.Length == 0) return string.Empty;
            if (!this.Contains(caretIndex, recognitionWay)) return string.Empty;

            return this.Data.Insert(caretIndex - this.StartIndex, addString);
        }

        public string MergeStringToFront(string addString) => this.Data.Insert(0, addString);
        public string MergeStringToEnd(string addString) => this.Data.Insert(this.Data.Length, addString);


        public TokenCell GetTokenInfoAfterMergeString(int caretIndex, string addString, RecognitionWay recognitionWay)
        {
            string mergeString = this.MergeString(caretIndex, addString, recognitionWay);
            if (mergeString.Length == 0) return null;

            return new TokenCell(this.StartIndex, mergeString, TokenPatternInfo.NotDefinedToken);
        }

        public override string ToString()
        {
            string convertedString = this.Data.Replace("\r", "\\r");
            convertedString = convertedString.Replace("\n", "\\n");
            convertedString = convertedString.Replace("\t", "\\t");

            return string.Format("{0}, \"{1}\", {2}", this.StartIndex, convertedString, PatternInfo.OptionData);
        }
    }


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
