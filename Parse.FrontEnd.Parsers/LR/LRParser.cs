using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.ParseTree;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract class LRParser : Parser
    {
        public abstract CanonicalTable C0 { get; }

        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public abstract event EventHandler<ParsingUnit> ActionSuccessed;
        public abstract event EventHandler<ParsingUnit> ReduceAction;
        public abstract event EventHandler<ParsingUnit> GotoAction;
        public abstract event EventHandler<ParsingUnit> ShiftAction;

        /// <summary>
        /// The Error Handler that if the action failed.
        /// ParsingFailResult : The state information when error generated
        /// </summary>
        public abstract event EventHandler<ParsingUnit> ActionFailed;

        public abstract SuccessedKind BlockParsing(ParsingBlock parsingBlock, bool bFromLastNext = true);
        public abstract SuccessedKind RecoveryBlockParsing(ParsingBlock parsingBlock, IReadOnlyList<ParsingRecoveryData> recoveryTokenInfos);


        /// <summary>
        /// The Error Handler that if the goto failed.
        /// TokenData : input data
        /// NonTerminalSet : possible nonterminal set
        /// </summary>
        public Action<TokenData, HashSet<NonTerminal>> GotoFailed { get; set; } = null;
        public enum SuccessedKind { Completed, ReduceOrGoto, Shift, NotApplicable };

        protected LRParser(Grammar grammar) : base(grammar)
        {
        }


        /// <summary>
        /// This function returns TokenData list converted from TokenCell list.
        /// </summary>
        /// <param name="tokenCells">The token cell list to convert</param>
        /// <returns>The TokenData list converted</returns>
        protected IReadOnlyList<TokenData> ToTokenDataList(IReadOnlyList<TokenCell> tokenCells)
        {
            if (tokenCells.Count == 0) return null;
            var result = new TokenData[tokenCells.Count + 1];

            for (int i = 0; i < tokenCells.Count; i++)
            //            Parallel.For(0, tokenCells.Count, (i) =>
            {
                var tokenCell = tokenCells[i];

                //****  this function may creates a key for TokenType (NotDefined) so this function is not thread safe. ****
                result[i] = TokenData.CreateFromTokenCell(tokenCell, (i == tokenCells.Count - 1));
            }

            var endMarker = new EndMarker();
            result[result.Length - 1] = new TokenData(endMarker, new TokenCell(-1, endMarker.Value, null));

            return result;
        }

        /// <summary>
        /// This function returns TokenData list converted from TokenCell list (as much as changedRanges range).
        /// </summary>
        /// <param name="tokenCells"></param>
        /// <param name="changedRanges"></param>
        /// <returns>The TokenData list converted</returns>
        protected IReadOnlyList<TokenData> ToTokenDataList(IReadOnlyList<TokenCell> tokenCells, TokenizeImpactRanges changedRanges)
        {
            if (tokenCells.Count == 0) return null;
            var result = new List<TokenData>();

            foreach (var range in changedRanges)
            {
                var curRange = range.Item2;
                for (int i = curRange.StartIndex; i < curRange.EndIndex + 1; i++)
                {
                    var tokenCell = tokenCells[i];
                    result.Add(TokenData.CreateFromTokenCell(tokenCell, (i == tokenCells.Count - 1)));
                }
            }

            return result;
        }
    }
}
