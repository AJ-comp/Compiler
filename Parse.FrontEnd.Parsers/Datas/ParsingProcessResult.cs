using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas
{
    public abstract class ParsingProcessResult
    {
        /// <summary> This property value is always valid. </summary>
        public ParsingUnit BlockItem { get; }
        /// <summary> This property value is always valid. </summary>
        public TokenData InputValue { get; }

        /// <summary> This property value is not valid in a specific case. </summary>
        public TerminalSet PossibleTerminalSet { get; }

        protected ParsingProcessResult(ParsingUnit blockItem, TokenData inputValue, TerminalSet possibleTerminalSet)
        {
            BlockItem = blockItem;
            InputValue = inputValue;
            PossibleTerminalSet = possibleTerminalSet;
        }
    }
}
