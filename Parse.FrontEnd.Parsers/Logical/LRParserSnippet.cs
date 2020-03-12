using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.Parsers.ErrorHandling;
using Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Logical
{
    public class LRParserSnippet : ParserSnippet
    {
        private enum SuccessedKind { Completed, ReduceOrGoto, Shift, NotApplicable };

        private int curTokenIndex = 0;
        private TokenData prevToken = null;

        /// <summary>
        /// The Error Handler that if the action failed.
        /// ParsingFailResult : The state information when error generated
        /// </summary>
        public event EventHandler<ParsingUnit> ActionFailed;
        /// <summary>
        /// The Error Handler that if the goto failed.
        /// TokenData : input data
        /// NonTerminalSet : possible nonterminal set
        /// </summary>
        public Action<TokenData, HashSet<NonTerminal>> GotoFailed { get; set; } = null;
        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public event EventHandler<ParsingUnit> ActionSuccessed;

        public LRParserSnippet(Parser parser) : base(parser)
        {
        }


        /// <summary>
        /// This function writes a value into stack following to action value (matchedValue).
        /// </summary>
        /// <param name="matchedValue"></param>
        /// <param name="inputValue"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private ActionData Process(Tuple<ActionDir, object> matchedValue, TokenData inputValue, Stack<object> stack)
        {
            ActionData result = new ActionData
            {
                Dest = matchedValue.Item2
            };

            if (matchedValue.Item1 == ActionDir.shift)
            {
                stack.Push(inputValue);
                stack.Push(matchedValue.Item2);

                result.Direction = ActionDir.shift;
            }
            else if (matchedValue.Item1 == ActionDir.reduce)
            {
                var reduceDest = matchedValue.Item2 as NonTerminalSingle;

                for (int i = 0; i < reduceDest.Count * 2; i++) stack.Pop();
                stack.Push(reduceDest);
                result.Direction = ActionDir.reduce;
            }
            else if (matchedValue.Item1 == ActionDir.epsilon_reduce)
            {
                stack.Push(matchedValue.Item2 as NonTerminalSingle);

                result.Direction = ActionDir.epsilon_reduce;
            }
            else if (matchedValue.Item1 == ActionDir.accept) result.Direction = ActionDir.accept;

            return result;
        }

        /// <summary>
        /// This function processes shift and reduce process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="parsingUnit"></param>
        /// <returns></returns>
        private bool ShiftAndReduce(TokenData inputValue, ParsingUnit parsingUnit)
        {
            parsingUnit.ChangeToNormalState();
            var topData = parsingUnit.BeforeStack.Peek();

            if (topData is NonTerminalSingle)
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)topData];

            // invalid input symbol, can't shift (error handler also not exist)
            if (!IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind))
            {
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState();
                return false;
            }
            // invalid input symbol, can't shift (error handler exists)
            else if (IxMetrix.MatchedValueSet[inputValue.Kind].Item2.GetType() == typeof(ErrorHandler))
            {
                var value = IxMetrix.MatchedValueSet[inputValue.Kind];
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState(value.Item2 as ErrorHandler);
                return false;
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];
            parsingUnit.Action = this.Process(matchedValue, inputValue, parsingUnit.AfterStack);

            parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
            return true;
        }

        /// <summary>
        /// This function processes goto process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="parsingUnit"></param>
        /// <returns></returns>
        private bool GoTo(TokenData inputValue, ParsingUnit parsingUnit)
        {
            parsingUnit.ChangeToNormalState();
            var topData = parsingUnit.BeforeStack.Peek();

            if (!(topData is NonTerminalSingle))
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            var seenSingleNT = topData as NonTerminalSingle;
            var secondData = parsingUnit.BeforeStack.SecondItemPeek();
            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)secondData];

            // can't goto action
            if (!IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal()))
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            var matchedValue = IxMetrix.MatchedValueSet[seenSingleNT.ToNonTerminal()];

            var actionData = new ActionData(matchedValue.Item1, matchedValue.Item2);
            parsingUnit.AfterStack.Push((int)matchedValue.Item2);
            parsingUnit.Action = actionData;
            parsingUnit.InputValue = inputValue;
            parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;

            return true;
        }

        /// <summary>
        /// This function returns a calculation result (it includes data about the next stack and how action did) with the current stack and input terminal.
        /// </summary>
        /// <param name="parsingBlock">The block of the 1 level parsing</param>
        /// <param name="inputValue">input terminal</param>
        /// <returns></returns>
        private ParsingUnit Parsing(ParsingBlock parsingBlock, TokenData inputValue)
        {
            ParsingUnit parsingUnit = parsingBlock.Units.Last();
            parsingUnit.InputValue = inputValue;
            parsingUnit.CopyBeforeStackToAfterStack();

            if(this.GoTo(inputValue, parsingUnit) == false)
                this.ShiftAndReduce(inputValue, parsingUnit);

            return parsingUnit;
        }

        /// <summary>
        /// This function is performed if a parsing process result is a success.
        /// </summary>
        /// <param name="successResult">The result of the 1 level parsing</param>
        /// <returns></returns>
        private SuccessedKind ParsingSuccessedProcess(ParsingUnit successResult)
        {
            SuccessedKind result = SuccessedKind.NotApplicable;

            // syntax analysis complete
            if (successResult.Action.Direction == ActionDir.accept)
            {
                result = SuccessedKind.Completed;
                //                this.Parser.Grammar.SDTS.Process(parsingResult.MeaningStack.Pop() as TreeSymbol);
            }
            else if (successResult.Action.Direction == ActionDir.reduce ||
                        successResult.Action.Direction == ActionDir.epsilon_reduce ||
                        successResult.Action.Direction == ActionDir.moveto)
            {
                result = SuccessedKind.ReduceOrGoto;
                this.ActionSuccessed?.Invoke(this, successResult);
            }
            else if(successResult.Action.Direction == ActionDir.shift)  result = SuccessedKind.Shift;

            return result;
        }

        /// <summary>
        /// This function is performed if parsing process result is a fail.
        /// </summary>
        /// <param name="parsingResult">The result of the whole parsing</param>
        /// <param name="tokens">The whole tokens, this needs to recover an error.</param>
        /// <returns>Returns ErrorHandlingResult if an error was recovered (this means that it can parse continue) else returns null.</returns>
        private ErrorHandlingResult ParsingFailedProcess(ParsingResult parsingResult, IReadOnlyList<TokenCell> tokens)
        {
            ErrorHandlingResult result = null;
            ParsingUnit lastParsingUnit = parsingResult.Last().Units.Last();

            this.ActionFailed?.Invoke(this, lastParsingUnit);

            if (lastParsingUnit.ErrorHandler != null)
                result = lastParsingUnit.ErrorHandler.Call(parsingResult, tokens.ToArray(), curTokenIndex);

            return result;
        }

        private ParsingResult ParsingCore(TokenCell[] tokenCells, ParsingResult parsingResult)
        {
            bool bBlockAdd = true;
            ParsingResult result = (parsingResult == null) ? new ParsingResult() : parsingResult;
            if (tokenCells.Length <= 0) return result;

            var tokens = tokenCells.ToList();
            for (int i = 0; i < tokens.Count; i++)
            {
                this.curTokenIndex = i;
                var item = tokens[i];
                if (bBlockAdd)
                {
                    var parsingUnit = (result.Count == 0) ? new ParsingUnit() : new ParsingUnit(result.Last().Units.Last().AfterStack);
                    result.Add(new ParsingBlock(parsingUnit, item));    // add new parsing block.
                }
                else
                {
                    result.Last().AddParsingItem(); // add new parsing item on current parsing block.
                }
                bBlockAdd = true;

                var type = Parser.GetTargetTerminalFromTokenCell(item, i == tokens.Count - 1);
                if (type == null)
                {
                    prevToken = new TokenData(item.Data, new Epsilon(), item);
                    parsingResult.Last().Units.Last().CopyBeforeStackToAfterStack();
                    continue; 
                }
                var token = new TokenData(item.Data, type, item);

                if (token.Kind == new NotDefined())
                {
                }

                var processResult = this.Parsing(result.Last(), token);
                var successedType = this.ParsingSuccessedProcess(processResult);

                if (successedType == SuccessedKind.Completed) break;
                else if (successedType == SuccessedKind.ReduceOrGoto)
                {
                    i--;
                    bBlockAdd = false;
                }
                else if(successedType == SuccessedKind.Shift) { }
                else
                {
                    var errorRecoverInfo = this.ParsingFailedProcess(parsingResult, tokens);
                    if (errorRecoverInfo == null) break;

                    i = errorRecoverInfo.SeeingTokenIndex;
                    if (i > 0) i--;
                }

                prevToken = token;
            }

            return result;
        }

        public override ParsingResult Parsing(TokenCell[] tokenCells)
        {
            if (tokenCells.Length <= 0) return new ParsingResult();

            var tokens = tokenCells.ToList();
            tokens.Add(new TokenCell(-1, new EndMarker().Value, null));

            var result = new ParsingResult();
            return this.ParsingCore(tokens.ToArray(), result);
        }

        /*
        public ParsingResult Parsing(TokenCell[] tokenCells, ParsingResult prevParsingInfo, TokenizeImpactRanges rangeToParse)
        {
            LRParsingResult result = new LRParsingResult();
            var totalList = prevParsingInfo.ParsingStack.Count;

            foreach (var range in rangeToParse.CurRanges)
            {
                var tokensToParse = tokenCells.Skip(range.StartIndex).Take(range.Count);
                var prevParsingStack = prevParsingInfo.ParsingStack.Take(range.EndIndex);

                result = this.ParsingCore(tokensToParse.ToArray(), ) as LRParsingResult;
            }
        }
        */
    }
}
