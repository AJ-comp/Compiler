using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers
{
    public class Lexer
    {
        private string source = string.Empty;
        private int codePieceIndex = 0;
        private Grammar grammar = null;
        private int strStartIndex = 0;
        private List<string> codePieces { get; } = new List<string>();

        /// <summary>
        /// Get the next terminal
        /// </summary>
        public TokenData NextToken
        {
            get
            {
                string value = string.Empty;

                while(true)
                {
                    value = this.GetNextString();
                    if (this.IsIgnoreString(value) == false) break;
                }

                return this.GetTokenInfo(value);

                /*
                get
                {
                    string value = (this.codePieceIndex >= this.codePieces.Count) ? string.Empty : this.codePieces[this.codePieceIndex++];

                    return new TokenData(value, (value == string.Empty) ? new Epsilon() : this.grammar.GetTerminal(value));
                }
                */
            }
        }

        public Lexer(Grammar grammar)
        {
            this.grammar = grammar;
        }

        private string GetNextString()
        {
            string result = string.Empty;
            bool bExistDelimitString = false;

            foreach (char input in this.source.Substring(this.strStartIndex))
            {
                string inputString = input.ToString();

                if (this.grammar.DelimiterDic.ContainsKey(inputString) == false)
                {
                    if (bExistDelimitString) break;

                    result += inputString;
                }
                else
                {
                    if (this.grammar.DelimiterDic.ContainsKey(result + inputString))
                    {
                        bExistDelimitString = true;
                        result += inputString;
                    }
                    else break;
                }
            }

            this.strStartIndex += result.Length;

            return result;
        }

        public void RollBackTokenReadIndex()
        {
            if(this.codePieceIndex > 0) this.codePieceIndex--;
        }

        public TokenData GetTokenInfo(string data)
        {
            return new TokenData(data, (data == string.Empty) ? new Epsilon() : this.grammar.GetTerminal(data));
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

        public void AddCode(string code)
        {
            if (this.source.Length > 0) this.source = this.source.Substring(0, this.source.Length - 1);

            this.source += code + new EndMarker().Value;
        }

        public void AddCode(int startIndex, string code)
        {
            if (startIndex >= this.source.Length) { this.AddCode(code); return; }

            this.source.Insert(startIndex, code);

            if (this.strStartIndex <= startIndex)
            {
                this.strStartIndex = startIndex;

                while (this.strStartIndex > 0)
                {
                    if (this.grammar.DelimiterDic.ContainsKey(this.source[--this.strStartIndex].ToString())) break;
                }
            }
        }
        public void DelCode(int startIndex, string code)
        {
            this.source.Remove(startIndex, code.Length);

            if (this.strStartIndex > startIndex)
            {
                this.strStartIndex = startIndex;

                while (this.strStartIndex > 0)
                {
                    if (this.grammar.DelimiterDic.ContainsKey(this.source[--this.strStartIndex].ToString())) break;
                }
            }
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
