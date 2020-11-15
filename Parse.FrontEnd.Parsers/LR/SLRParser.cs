﻿using Parse.Extensions;
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

        public void PartialParsing(ParsingResult target, IReadOnlyList<RangePair> rangesToParse)
        {
            foreach (var range in rangesToParse)
            {
                int startIndex = (!range.Item1.IsEmpty) ? range.Item1.StartIndex : range.Item2.StartIndex;
                if (startIndex < 0) continue;

                // parsing update range.
                for (int i = startIndex; i <= target.Count; i++)
                {
                    // check if it has to parsing
                    // if 0 index token was modified it always has to parsing.
                    if (i > 0)
                    {
                        if (target.IsRightBlockConnected(i - 1)) continue;
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
                for (int i = rangePair.Item1.StartIndex; i < rangePair.Item1.EndIndex; i++)
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
