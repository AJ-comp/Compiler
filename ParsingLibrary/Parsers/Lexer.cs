using ParsingLibrary.Datas;
using ParsingLibrary.Datas.RegularGrammar;
using ParsingLibrary.Grammars;
using System.Collections.Generic;

namespace ParsingLibrary.Parsers
{
    public class Lexer
    {
        private string source = string.Empty;
        private int codePieceIndex = 0;
        private Grammar grammar = null;
        private Stack<int> codePieceStack = new Stack<int>();

        private List<string> codePieces { get; } = new List<string>();

        /// <summary>
        /// Get the next terminal
        /// </summary>
        public TokenData NextToken
        {
            get
            {
                string value = (this.codePieceIndex >= this.codePieces.Count) ? string.Empty : this.codePieces[this.codePieceIndex++];

                return new TokenData(value, (value == string.Empty) ? new Epsilon() : this.grammar.GetTerminal(value));
            }
        }

        public Lexer(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public void RollBackTokenReadIndex()
        {
            if(this.codePieceIndex > 0) this.codePieceIndex--;
        }

        public void SetCode(string source)
        {
            this.source = source + new EndMarker().Value;

            string data = string.Empty;

            foreach (char ch in this.source)
            {
                if (this.grammar.DelimiterDic.ContainsKey(ch))
                {
                    if(data.Length > 0) this.codePieces.Add(data);
                    if(this.grammar.DelimiterDic[ch] == false)  this.codePieces.Add(ch.ToString());

                    data = string.Empty;
                }
                else data += ch;
            }

            if (data.Length > 0) this.codePieces.Add(data);
        }

        public void BeginCodePiece()
        {
            this.codePieceStack.Push(this.codePieceIndex);
        }

        public void TransCodePiece()
        {
            this.codePieceIndex = this.codePieceStack.Pop();
        }

        public void CommitCodePiece()
        {
            this.codePieceStack.Pop();
        }
    }
}
