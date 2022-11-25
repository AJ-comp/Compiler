using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Parse.FrontEnd.Tokenize
{
    public enum RecognitionWay { Front, Back };

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TokenCell
    {
        public int StartIndex { get; internal set; }
        public int EndIndex => this.StartIndex + this.Data.Length - 1;
        public int StartLineIndex { get; internal set; }
        public int EndLineIndex => StartLineIndex + Regex.Matches(Data, "\n").Count;
        public int StartColumnIndex { get; }
        public int EndColumnIndex
        {
            get
            {
                if (StartLineIndex == EndLineIndex) return StartColumnIndex + Data.Length - 1;

                var colCountInNewLine = Data.Skip(Data.LastIndexOf("\n") + 1).Count();
                return (colCountInNewLine == 0) ? 0 : colCountInNewLine - 1;
            }
        }
        public string Data { get; } = string.Empty;
        public Match MatchData { get; }
        public TokenPatternInfo PatternInfo { get; internal set; }

        public ValueType ValueOptionData { get; set; } = 0;
        public object RefOptionalData { get; set; }

        /*
        public TokenCell(int startIndex, string data, TokenPatternInfo patternInfo)
        {
            this.StartIndex = startIndex;
            this.Data = data;

            this.PatternInfo = patternInfo;
        }
        */

        public TokenCell(int startIndex, int startLineIndex, int startColIndex, string data, Match matchData)
        {
            this.StartIndex = startIndex;
            this.Data = data;
            this.StartLineIndex = startLineIndex;
            this.StartColumnIndex = startColIndex;

            this.MatchData = matchData;
        }

        public bool Contains(int caretIndex, RecognitionWay recognitionWay)
        {
            return (recognitionWay == RecognitionWay.Back)
                    ? (this.StartIndex < caretIndex && caretIndex <= this.EndIndex + 1)
                    : (this.StartIndex <= caretIndex && caretIndex <= this.EndIndex);
        }

        public bool Contains(int tokenIndex) => this.StartIndex <= tokenIndex && tokenIndex <= this.EndIndex;

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


        public bool Equals(TokenCell other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return (this.GetHashCode() == other.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is TokenCell)
            {
                TokenCell right = obj as TokenCell;

                result = (this.StartIndex == right.StartIndex && this.Data == right.Data);
            }

            return result;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartIndex, EndIndex, PatternInfo);
        }

        public static bool operator ==(TokenCell cell1, TokenCell cell2)
        {
            return EqualityComparer<TokenCell>.Default.Equals(cell1, cell2);
        }

        public static bool operator !=(TokenCell cell1, TokenCell cell2)
        {
            return !(cell1 == cell2);
        }


        private string DebuggerDisplay
        {
            get
            {
                string convertedString = this.Data.Replace("\r", "\\r");
                convertedString = convertedString.Replace("\n", "\\n");
                convertedString = convertedString.Replace("\t", "\\t");

                var result =  (PatternInfo.Terminal == null) ? $"{this.StartIndex}, \"{convertedString}\", null"
                                                                                : $"{this.StartIndex}, \"{convertedString}\", {PatternInfo.Terminal}";

                return result;
            }
        }
    }
}
