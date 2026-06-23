using ParsingLibrary.Datas.RegularGrammar;
using System.Collections.Generic;

namespace ParsingLibrary.Utilities
{
    public static class TokenKeyManager
    {
        static int nextKey = 1;
        static List<TerminalData> terminals = new List<TerminalData>();

        public static int CreateKey(Terminal terminal)
        {
            int result = 0;
            TerminalData terminalData = new TerminalData(terminal.TokenType, terminal.Value, nextKey);

            foreach(var data in terminals)
            {
                if (data.type == terminalData.type && data.value == terminalData.value)
                {
                    result = data.key;
                    break;
                }
            }

            if (result == 0)
            {
                terminals.Add(terminalData);
                result = nextKey++;
            }

            return result;
        }
    }

    internal class TerminalData
    {
        internal TokenType type;
        internal string value;
        internal int key;

        public TerminalData(TokenType type, string value, int key)
        {
            this.type = type;
            this.value = value;
            this.key = key;
        }

    }
}
