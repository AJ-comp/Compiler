using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.Parsers.RelationAnalyzers;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser : Parser
    {
        /// <summary>
        /// The Error Handler that if the goto failed.
        /// TokenData : input data
        /// NonTerminalSet : possible nonterminal set
        /// </summary>
        public Action<TokenData, HashSet<NonTerminal>> GotoFailed { get; set; } = null;
        public enum SuccessedKind { Completed, ReduceOrGoto, Shift, NotApplicable };

        public CanonicalRelation Canonical { get; } = new CanonicalRelation();
        public IErrorHandlable ErrorHandler { get; private set; }

        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public event EventHandler<ParsingUnit> ActionSuccessed;
        public event EventHandler<ParsingUnit> ReduceAction;
        public event EventHandler<ParsingUnit> GotoAction;
        public event EventHandler<ParsingUnit> ShiftAction;
        public override event EventHandler<ParseCreatedArgs> ParseTreeCreated;
        public override event EventHandler<AstSymbol> ASTCreated;

        public override IParsingTable ParsingTable { get; } = new LRParsingTable();
        public override string AnalysisResult => this.Canonical.ToString();

        /// <summary>
        /// The Error Handler that if the action failed.
        /// ParsingFailResult : The state information when error generated
        /// </summary>
        public event EventHandler<ParsingUnit> ActionFailed;

        public abstract AmbiguityCheckResult CheckAmbiguity();


        public override FirstAndFollowCollection GetFirstAndFollow()
        {
            var result = new FirstAndFollowCollection();

            foreach (var symbol in ParsingTable.AllSymbols)
            {
                if (symbol is NonTerminal)
                    result.Add(new FirstAndFollowItem(symbol, Analyzer.FirstTerminalSet(symbol), _followAnalyzer.Follow(symbol as NonTerminal)));
                else
                    result.Add(new FirstAndFollowItem(symbol, Analyzer.FirstTerminalSet(symbol), new TerminalSet()));
            }

            return result;
        }


        protected FollowAnalyzer _followAnalyzer = new FollowAnalyzer();
        protected Stack<IEnumerable<AstSymbol>> _astStack = new Stack<IEnumerable<AstSymbol>>();

        protected LRParser(Grammar grammar, ReduceParameter reduceParameter) : base(grammar)
        {
            DataTableExtensionMethods.ASCIIBorder();

            var virtualStartSymbol = this.Grammar.CreateVirtualSymbolForLRParsing("Accept");
            _followAnalyzer.CalculateAllFollow(this.Grammar.NonTerminalMultiples);
            Canonical.Calculate(virtualStartSymbol, _followAnalyzer.Datas);
            Canonical.ReduceParameter = reduceParameter;
            ParsingTable.CreateParsingTable(this.Canonical);
        }


        public LRParser AddErrorHandler(IErrorHandlable errorHandler)
        {
            ErrorHandler = errorHandler;
            return this;
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
            result[^1] = new TokenData(endMarker, new TokenCell(-1, endMarker.Value, null));

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
            if (successResult.Action.Direction == ActionDir.Accept)
            {
                result = SuccessedKind.Completed;
            }
            else if (successResult.Action.Direction == ActionDir.Reduce ||
                        successResult.Action.Direction == ActionDir.EpsilonReduce ||
                        successResult.Action.Direction == ActionDir.Goto)
            {
                result = SuccessedKind.ReduceOrGoto;
                this.ActionSuccessed?.Invoke(this, successResult);
            }
            else if (successResult.Action.Direction == ActionDir.Shift) result = SuccessedKind.Shift;

            return result;
        }



        public void PartialParsing(ParsingResult target, IReadOnlyList<RangePair> rangesToParse)
        {
            foreach (var range in rangesToParse)
            {
                int startIndex = (!range.Item1.IsEmpty) ? range.Item1.StartIndex : range.Item2.StartIndex;
                if (startIndex < 0) continue;

                // parsing only update range.
                for (int i = startIndex; i <= target.Count; i++)
                {
                    // check if it has to parsing
                    // if 0 index token was modified it always has to parsing.
                    if (i > 0)
                    {
                        if (target.JoinBlock(i - 1))
                        {
                            // at least it has to see to the last index of range.
                            if (i > range.Item2.EndIndex) break;
                            else continue;
                        }
                    }

                    if (i == 0) target[i] = new ParsingBlock(target[i].Token);
                    else target[i] = new ParsingBlock(new ParsingUnit(target[i - 1].Units.Last().AfterStack), target[i].Token);

                    var successKind = this.BlockParsing(target, i);
                    this.PostProcessing(successKind, target, true, ref i);
                }
            }
        }

        /// <summary>
        /// This function performs the following process
        /// It parsing next to 'basis' from 'seeingIndex' of 'wholeTokens' as much as 'rangeToParse'.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private void AllParsing(ParsingResult target)
        {
            for (int i = 0; i < target.Count; i++)
            {
                var blockParsingResult = this.BlockParsing(target, i);

                if (this.PostProcessing(blockParsingResult, target, true, ref i)) break;
            }
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenCell> tokenCells)
        {
            var tokens = ToTokenDataList(tokenCells);

            ParsingResult result = new ParsingResult();
            try
            {
                if (tokens == null || tokens.Count <= 0) return result;

                result = new ParsingResult();
                foreach (var item in tokens) result.Add(new ParsingBlock(item));

                this.AllParsing(result);
                result.Success = true;
            }
            catch (ParsingException ex)
            {
                result = new ParsingResult(result.Take(ex.SeeingIndex + 1))
                {
                    Success = false
                };
            }
            catch (Exception)
            {
                result.Success = false;
            }

            return result;
        }

        public override ParsingResult Parsing(LexingData lexingData, ParsingResult prevParsingInfo)
        {
            var tokenCells = lexingData.TokensForParsing;

            if (prevParsingInfo == null) return this.Parsing(tokenCells);
            if (prevParsingInfo.Success == false) return this.Parsing(tokenCells);
            if (lexingData.RangeToLex == null) return this.Parsing(tokenCells);

            ParsingResult result = null;
            var tokens = ToTokenDataList(tokenCells);

            try
            {
                var differRanges = CollectDifferRanges(prevParsingInfo, lexingData.RangeToParsing, tokens);

                if (differRanges.Count() > 0)
                {
                    result = ReplaceRanges(prevParsingInfo, differRanges, tokens);
                    PartialParsing(result, lexingData.RangeToParsing);
                }
                // if tokens to remove same with tokens to insert it doesn't need parsing.
                else result = prevParsingInfo;

                result.Success = true;
            }
            catch (ParsingException ex)
            {
                result = new ParsingResult(result.Take(ex.SeeingIndex + 1))
                {
                    Success = false
                };
            }
            catch (Exception)
            {
                result.Success = false;
            }

            return result;

        }

        /// <summary>
        /// This function is performed if parsing process result is a fail.
        /// </summary>
        /// <param name="parsingResult">The result of the whole parsing</param>
        /// <param name="tokens">The whole tokens, this needs to recover an error.</param>
        /// <param name="curTokenIndex">The token index seeing current</param>
        /// <returns>Returns ErrorHandlingResult if an error was recovered (this means that it can parse continue) else returns null.</returns>
        private ErrorHandlingResult ParsingFailedProcess(ParsingResult parsingResult, int curTokenIndex)
        {
            ParsingUnit lastParsingUnit = parsingResult[curTokenIndex].Units.Last();

            this.ActionFailed?.Invoke(this, lastParsingUnit);

            /*
            if (lastParsingUnit.ErrorHandler != null)
                result = lastParsingUnit.ErrorHandler.Call(new DataForRecovery(this, parsingResult, curTokenIndex));
            */

            return ErrorHandler?.Call(new DataForRecovery(this, parsingResult, curTokenIndex));
        }

        /// <summary>
        /// This function processes the result that be created after parsing.
        /// </summary>
        /// <param name="successedType">The result kind of the parsing</param>
        /// <param name="parsingResult">The whole result of the parsing until the current</param>
        /// <param name="bErrorRecover"></param>
        /// <param name="index"></param>
        /// <returns>Return true if a token that must to parse does not exist (Completed), else Return false.</returns>
        /// <exception cref="">If error handling failed then throw exception.</exception>
        private bool PostProcessing(SuccessedKind successedType, ParsingResult parsingResult, bool bErrorRecover, ref int index)
        {
            bool result = false;

            // ReduceOrGoto state can't arrive in this function because it is processed in 'BlockParsing' function.
            // Therefore this function doesn't process ReduceOrGoto state.
            if (successedType == SuccessedKind.Completed) result = true;
            else if (successedType == SuccessedKind.Shift) { }
            else if (bErrorRecover)
            {
                var errorRecoverInfo = this.ParsingFailedProcess(parsingResult, index);
                if (errorRecoverInfo == null) throw new ParsingException(index);
                else if (errorRecoverInfo.SuccessRecover == false) throw new ParsingException(errorRecoverInfo.TokenIndexToSee);
                else
                {
                    index = errorRecoverInfo.TokenIndexToSee;
                    //                    if (index > 0) index--;
                }
            }

            return result;
        }

        private IEnumerable<RangePair> CollectDifferRanges(ParsingResult srcResult, IReadOnlyList<RangePair> rangePairs, IReadOnlyList<TokenData> newTokens)
        {
            List<RangePair> result = new List<RangePair>();

            foreach (var rangePair in rangePairs)
            {
                if (!rangePair.Same)
                {
                    result.Add(rangePair);
                    continue;
                }

                // rangePair is same
                for (int i = rangePair.Item1.StartIndex; i < rangePair.Item1.EndIndex + 1; i++)
                {
                    if (srcResult[i].Token.Input != newTokens[i].Input) result.Add(rangePair);
                }
            }

            return result;
        }

        private ParsingResult ReplaceRange(ParsingResult srcResult, Range range, IReadOnlyList<TokenData> newTokens, Range newRange)
        {
            var prevParsingResult = new ParsingResult(srcResult.Take(range.StartIndex));
            var postParsingResult = new ParsingResult(srcResult.Skip(range.EndIndex + 1));

            for (int i = newRange.StartIndex; i <= newRange.EndIndex; i++)
            {
                prevParsingResult.Add(new ParsingBlock(newTokens[i]));
            }

            prevParsingResult.AddRange(postParsingResult);

            return prevParsingResult;
        }

        private ParsingResult ReplaceRanges(ParsingResult srcResult, IEnumerable<RangePair> rangePairs, IReadOnlyList<TokenData> newTokens)
        {
            foreach (var rangePair in rangePairs)
            {
                var removeRange = rangePair.Item1;
                if (!removeRange.IsEmpty) srcResult.RemoveRange(removeRange.StartIndex, removeRange.Count);

                var insertRange = rangePair.Item2;
                if (!insertRange.IsEmpty)
                {
                    for (int i = insertRange.StartIndex; i <= insertRange.EndIndex; i++)
                    {
                        srcResult.Insert(i, new ParsingBlock(newTokens[i]));
                    }
                }

                /*
                var adjustRange = new Range(rangePair.Item1.StartIndex + addVal, rangePair.Item1.Count);

                srcResult = this.ReplaceRange(srcResult, adjustRange, newTokens, rangePair.Item2);
                addVal += rangePair.Item2.Count - rangePair.Item1.Count;
                */
            }

            return srcResult;
        }
    }
}
