using Parse.FrontEnd.Grammars;
using Parse.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers
{
    public class Lexer
    {
        private string source = string.Empty;
        private int codePieceIndex = 0;
        private Grammar grammar = null;
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

        /// <summary>
        /// This function returns value that whether ignore when collecting string.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool IsIgnoreString(string target)
        {
            bool result = false;

            if (this.grammar.DelimiterDic.ContainsKey(target))  result = this.grammar.DelimiterDic[target];

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <see cref="https://www.lucidchart.com/documents/edit/41d20574-d843-41ce-ae44-2d3e29fbc716/0?beaconFlowId=E9C38964142A86C1"/>
        public void SetCode(string source)
        {
            this.codePieceIndex = 0;
            this.codePieces.Clear();
            this.source = source + new EndMarker().Value;

            string data = string.Empty;
            string delimiteStr = string.Empty;

            foreach (char input in this.source)
            {
                string inputString = input.ToString();

                if (this.grammar.DelimiterDic.ContainsKey(delimiteStr + inputString))
                {
                    if (!this.IsIgnoreString(inputString)) delimiteStr += inputString;

                    if (data.Length > 0)
                    {
                        this.codePieces.Add(data);
                        data = string.Empty;
                    }

                    continue;
                }

                if (delimiteStr.Length > 0)
                {
                    this.codePieces.Add(delimiteStr);
                    delimiteStr = string.Empty;
                }

                if (this.grammar.DelimiterDic.ContainsKey(inputString))
                {
                    if(!this.grammar.DelimiterDic[inputString]) delimiteStr = input.ToString();
                }
                else data += input;
            }

            if (delimiteStr.Length > 0) this.codePieces.Add(delimiteStr);
//            if (data.Length > 0) this.codePieces.Add(data);
        }
    }
}
