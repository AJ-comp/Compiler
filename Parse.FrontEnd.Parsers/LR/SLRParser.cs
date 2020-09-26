using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.RelationAnalyzers;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.LR
{
    public class SLRParser : LRParser
    {
        private FollowAnalyzer _followAnalyzer = new FollowAnalyzer();
        private Stack<IEnumerable<AstSymbol>> _astStack = new Stack<IEnumerable<AstSymbol>>();

        public override event EventHandler<ParsingUnit> ActionFailed;

        public override IParsingTable ParsingTable { get; } = new LRParsingTable();
        public override string AnalysisResult => this.C0.ToString();
        public override CanonicalTable C0 { get; } = new CanonicalTable();

        public SLRParser(Grammar grammar) : base(grammar)
        {
            DataTableExtensionMethods.ASCIIBorder();

            var virtualStartSymbol = this.Grammar.CreateVirtualSymbolForLRParsing("Accept");
            C0.Calculate(virtualStartSymbol);
            _followAnalyzer.CalculateAllFollow(this.Grammar.NonTerminalMultiples);
            ParsingTable.CreateParsingTable(this.C0, this._followAnalyzer.Datas);
        }

        public void PartialParsing(ParsingResult target, TokenizeImpactRanges rangesToParse)
        {
            int indexToSee = 0;

            foreach (var range in rangesToParse)
            {
                if (range.Item2.StartIndex < indexToSee && indexToSee < range.Item2.EndIndex)
                {
                }
                else if (indexToSee > range.Item2.EndIndex) continue;

                // parsing update range.
                for (int i = range.Item2.StartIndex; i <= range.Item2.EndIndex; i++)
                {
                    if (i == 0) target[i] = new ParsingBlock(target[i].Token);
                    else target[i] = new ParsingBlock(new ParsingUnit(target[i - 1].Units.Last().AfterStack), target[i].Token);

                    var successKind = this.BlockParsing(target, i);
                    this.PostProcessing(successKind, target, true, ref i);

                    indexToSee = i + 1;
                }

                // parsing for post range until sync.
                for (int i = indexToSee; i < target.Count; i++)
                {
                    var targetToCompare = target[i];
                    if(targetToCompare.Units.First().BeforeStack.Stack.SequenceEqual(target[i].Units.Last().AfterStack.Stack))
                    {
                        indexToSee = i + 1;
                        break;
                    }

                    target[i] = new ParsingBlock(new ParsingUnit(target[i - 1].Units.Last().AfterStack), target[i].Token);
                    var successKind = this.BlockParsing(target, i);

                    this.PostProcessing(successKind, target, true, ref i);
                    indexToSee = i + 1;
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
                if (tokens == null) return result;
                if (tokens.Count <= 0) return result;

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
            var tokenCells = lexingData.TokenStorage.TokensToView;

            if (prevParsingInfo == null) return this.Parsing(tokenCells);
            if (prevParsingInfo.Success == false) return this.Parsing(tokenCells);
            if (lexingData.RangeToParse == null) return this.Parsing(tokenCells);

            var tokens = ToTokenDataList(tokenCells);
            ParsingResult result = null;
            try
            {
                result = this.ReplaceRanges(prevParsingInfo, tokens, lexingData.RangeToParse);
                this.PartialParsing(result, lexingData.RangeToParse);
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
            ErrorHandlingResult result = null;
            ParsingUnit lastParsingUnit = parsingResult[curTokenIndex].Units.Last();

            this.ActionFailed?.Invoke(this, lastParsingUnit);

            if (lastParsingUnit.ErrorHandler != null)
                result = lastParsingUnit.ErrorHandler.Call(this, parsingResult, curTokenIndex);

            return result;
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

            ParsingBlock nextBlock = (parsingResult.Count > index) ? parsingResult[index] : null;

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

        private ParsingResult ReplaceRanges(ParsingResult srcResult, IReadOnlyList<TokenData> newTokens, TokenizeImpactRanges ranges)
        {
            int addVal = 0;

            foreach (var range in ranges)
            {
                var adjustRange = new Range(range.Item1.StartIndex + addVal, range.Item1.Count);

                srcResult = this.ReplaceRange(srcResult, adjustRange, newTokens, range.Item2);
                addVal += range.Item2.Count - range.Item1.Count;
            }

            return srcResult;
        }
    }
}
