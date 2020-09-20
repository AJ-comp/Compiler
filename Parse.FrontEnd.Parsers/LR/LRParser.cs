using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser : Parser
    {
        public abstract CanonicalTable C0 { get; }

        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public event EventHandler<ParsingUnit> ActionSuccessed;
        public event EventHandler<ParsingUnit> ReduceAction;
        public event EventHandler<ParsingUnit> GotoAction;
        public event EventHandler<ParsingUnit> ShiftAction;
        public override event EventHandler<ParseCreatedArgs> ParseTreeCreated;
        public override event EventHandler<AstSymbol> ASTCreated;

        /// <summary>
        /// The Error Handler that if the action failed.
        /// ParsingFailResult : The state information when error generated
        /// </summary>
        public abstract event EventHandler<ParsingUnit> ActionFailed;

        public abstract SuccessedKind BlockParsing(ParsingBlock parsingBlock);


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

        /// <summary>
        /// This function is performed if a parsing process result is a success.
        /// </summary>
        /// <param name="successResult">The result of the 1 level parsing</param>
        /// <returns></returns>
        protected SuccessedKind ParsingSuccessedProcess(ParsingUnit successResult)
        {
            SuccessedKind result = SuccessedKind.NotApplicable;

            // syntax analysis complete
            if (successResult.Action.Direction == ActionDir.accept)
            {
                result = SuccessedKind.Completed;
            }
            else if (successResult.Action.Direction == ActionDir.reduce ||
                        successResult.Action.Direction == ActionDir.epsilon_reduce ||
                        successResult.Action.Direction == ActionDir.moveto)
            {
                result = SuccessedKind.ReduceOrGoto;
                this.ActionSuccessed?.Invoke(this, successResult);
            }
            else if (successResult.Action.Direction == ActionDir.shift) result = SuccessedKind.Shift;

            return result;
        }
    }
}
