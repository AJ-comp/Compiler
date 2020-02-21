using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Logical
{
    public class LRParserSnippet : ParserSnippet
    {
        private int curTokenIndex = 0;
        private TokenData prevToken = null;

        /// <summary>
        /// The Error Handler that if the action failed.
        /// LRParsingEventArgs : The state information when error generated
        /// TerminalSet : The possible input terminal set
        /// </summary>
        public event EventHandler<LRParsingFailedEventArgs> ActionFailed;
        /// <summary>
        /// The Error Handler that if the goto failed.
        /// TokenData : input data
        /// NonTerminalSet : possible nonterminal set
        /// </summary>
        public Action<TokenData, HashSet<NonTerminal>> GotoFailed { get; set; } = null;
        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public event EventHandler<LRParsingEventArgs> ActionCompleted;

        protected override void CreateParsingHistoryTemplate()
        {
            this.ParsingHistory.AddColumn("prev stack");
            this.ParsingHistory.AddColumn("input symbol");
            this.ParsingHistory.AddColumn("action information");
            this.ParsingHistory.AddColumn("current stack");
        }

        public override string ToParsingTreeString()
        {
            string result = string.Empty;

            foreach (var item in this.ParsingHistory.TreeInfo.ToReverseList())
                result += item.ToTreeString();

            return result;
        }

        public LRParserSnippet(Parser parser) : base(parser)
        {
            //            this.parsingRule.ActionCompleted += this.BuildParseTree;
            this.ActionCompleted += this.AddParsingHistory;
            this.ActionFailed += this.AddFailedInfoToParsingHistory;
        }

        private void BuildParseTree(object sender, LRParsingEventArgs args)
        {
            if (args.ActionData.ActionDirection == ActionDir.shift)
            {
                //                if (!args.InputValue.Kind.Meaning) return;

                this.MeaningStack.Push(new AstTerminal(args.InputValue));
            }
            else if (args.ActionData.ActionDirection == ActionDir.reduce)
            {
                var item = args.ActionData.ActionDest as NonTerminalSingle;

                AstNonTerminal nonTerminal = new AstNonTerminal(item);
                for (int i = 0; i < item.Count; i++) nonTerminal.Insert(0, this.MeaningStack.Pop());
                this.MeaningStack.Push(nonTerminal);

                item.MeaningUnit?.ActionLogic(nonTerminal);
            }
            else if (args.ActionData.ActionDirection == ActionDir.epsilon_reduce)
            {
                var item = args.ActionData.ActionDest as NonTerminalSingle;
                AstNonTerminal nonTerminal = new AstNonTerminal(item);

                this.MeaningStack.Push(nonTerminal);

                item.MeaningUnit?.ActionLogic(nonTerminal);
            }
        }

        /// <summary>
        /// This function writes parsing process information to the parsingHistory.
        /// </summary>
        /// <param name="args">parsing process information</param>
        private void AddParsingHistory(object sender, LRParsingEventArgs args)
        {
            this.BuildParseTree(sender, args);

            var param1 = Convert.ToString(args.PrevStack.Reverse(), " ");
            var param2 = args.InputValue.ToString();
            var param3 = args.ActionData.ActionDirection.ToString() + " ";
            var param4 = Convert.ToString(args.CurrentStack.Reverse(), " ");

            if (args.ActionData.ActionDirection != ActionDir.accept)
                param3 += (args.ActionData.ActionDest is NonTerminalSingle) ? (args.ActionData.ActionDest as NonTerminalSingle).ToGrammarString() : args.ActionData.ActionDest.ToString();

            if (args.ActionData.ActionDirection == ActionDir.reduce)
                this.ParsingHistory.AddTreeInfo(args.ActionData.ActionDest as NonTerminalSingle);

            this.ParsingHistory.AddRow(param1, param2, param3, param4);
        }

        /// <summary>
        /// This function writes parsing error information to the parsingHistory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">parsing process information when error generated</param>
        private void AddFailedInfoToParsingHistory(object sender, LRParsingFailedEventArgs args)
        {
            var param1 = Convert.ToString(args.PrevStack.Reverse(), " ");
            var param2 = args.InputValue.ToString();
            //            var param3 = args.ActionData.ActionDirection.ToString() + " ";

            string message = Resource.CantShift + " " + args.PossibleSet + " " + Resource.MustCome;
            args.ErrorMessage = message;
            this.ParsingHistory.AddRow(param1, param2, message, string.Empty);

            if (args.InputValue.Kind == new EndMarker())
            {
                args.ErrorPosition = ErrorPosition.OnEndMarker;
                args = new LRParsingFailedEventArgs(args.PrevStack, args.CurrentStack, this.prevToken, args.ActionData, args.PossibleSet)
                {
                    ErrorIndex = this.curTokenIndex - 1   // because prev token index
                };
            }
            else
            {
                args.ErrorPosition = ErrorPosition.OnNormalToken;
                args.ErrorIndex = this.curTokenIndex;
            }

            this.OnParsingFailed(args);
        }





        private ActionData Process(Tuple<ActionDir, object> matchedValue, TokenData inputValue, Stack<object> stack)
        {
            ActionData result = new ActionData
            {
                ActionDest = matchedValue.Item2
            };

            if (matchedValue.Item1 == ActionDir.shift)
            {
                stack.Push(inputValue);
                stack.Push(matchedValue.Item2);

                result.ActionDirection = ActionDir.shift;
            }
            else if (matchedValue.Item1 == ActionDir.reduce)
            {
                var reduceDest = matchedValue.Item2 as NonTerminalSingle;

                for (int i = 0; i < reduceDest.Count * 2; i++) stack.Pop();
                stack.Push(reduceDest);
                result.ActionDirection = ActionDir.reduce;
            }
            else if (matchedValue.Item1 == ActionDir.epsilon_reduce)
            {
                stack.Push(matchedValue.Item2 as NonTerminalSingle);

                result.ActionDirection = ActionDir.epsilon_reduce;
            }
            else if (matchedValue.Item1 == ActionDir.accept) result.ActionDirection = ActionDir.accept;

            return result;
        }

        /// <summary>
        /// This function processes shift and reduce process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="prevStack"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private ActionDir ShiftAndReduce(TokenData inputValue, Stack<object> prevStack, Stack<object> stack)
        {
            var topData = stack.Peek();

            if (topData is NonTerminalSingle) return ActionDir.failed;

            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)topData];

            // invalid input symbol, can't shift
            if (!IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind))
            {
                var data = new LRParsingEventArgs(prevStack, stack, inputValue, new ActionData(ActionDir.failed, null));
                this.ActionFailed?.Invoke(this, new LRParsingFailedEventArgs(prevStack, stack, inputValue, new ActionData(ActionDir.failed, null), IxMetrix.PossibleTerminalSet));
                return ActionDir.failed;
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];
            var result = this.Process(matchedValue, inputValue, stack);

            this.ActionCompleted?.Invoke(this, new LRParsingEventArgs(prevStack, stack, inputValue, result));

            return result.ActionDirection;
        }

        /// <summary>
        /// This function processes goto process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="prevStack"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private bool GoTo(TokenData inputValue, Stack<object> prevStack, Stack<object> stack)
        {
            bool result = true;
            var topData = stack.Peek();

            if (!(topData is NonTerminalSingle)) return false;

            var seenSingleNT = topData as NonTerminalSingle;
            var secondData = stack.SecondItemPeek();
            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)secondData];

            // can't goto action
            if (!IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal()))
            {
                result = false;
                this.GotoFailed?.Invoke(inputValue, IxMetrix.PossibleNonTerminalSet);
            }
            else
            {
                var matchedValue = IxMetrix.MatchedValueSet[seenSingleNT.ToNonTerminal()];

                stack.Push((int)matchedValue.Item2);
                var actionData = new ActionData(matchedValue.Item1, matchedValue.Item2);
                this.ActionCompleted?.Invoke(this, new LRParsingEventArgs(prevStack, stack, inputValue, actionData));
            }

            return result;
        }


        /// <summary>
        /// This function returns a calculation result (it includes data about the next stack and how action did) with the current stack and input terminal.
        /// </summary>
        /// <param name="stack">current stack</param>
        /// <param name="inputValue">input terminal</param>
        /// <returns></returns>
        public LRParsingProcessResult Parsing(Stack<object> stack, TokenData inputValue)
        {
            ActionDir actionDir = ActionDir.failed;
            Stack<object> prevStack = stack.Clone();

            if (this.GoTo(inputValue, prevStack, stack)) actionDir = ActionDir.moveto;
            else actionDir = this.ShiftAndReduce(inputValue, prevStack, stack);

            return new LRParsingProcessResult(actionDir, stack);
        }

        public override bool Parsing(TokenCell[] tokenCells)
        {
            if (tokenCells.Length <= 0) return true;

            bool result = false;
            Stack<object> stack = new Stack<object>();
            stack.Push(0);

            var tokens = tokenCells.ToList();
            tokens.Add(new TokenCell(-1, new EndMarker().Value, null));

            for (int i = 0; i < tokens.Count; i++)
            {
                this.curTokenIndex = i;
                var item = tokens[i];

                Terminal type = new Epsilon();
                if (item.Data == new EndMarker().Value && i == tokens.Count - 1) type = new EndMarker();
                else
                {
                    var typeData = item.PatternInfo.OptionData as Terminal;
                    if (typeData == null) type = new NotDefined();
                    else if (typeData.TokenType == TokenType.Delimiter) { prevToken = new TokenData(item.Data, type, item); continue; }
                    else if (typeData.TokenType == TokenType.Comment) { prevToken = new TokenData(item.Data, type, item); continue; }
                    else type = typeData;
                }

                var token = new TokenData(item.Data, type, item);

                if (token.Kind == new NotDefined())
                {
                }
                var parsingResult = this.Parsing(stack, token);
                if (parsingResult.ActionDir == ActionDir.failed)
                {
                    result = false;
                    break;
                }
                else if (parsingResult.ActionDir == ActionDir.accept)
                {
                    result = true;
                    break;
                }
                else if (parsingResult.ActionDir == ActionDir.reduce || parsingResult.ActionDir == ActionDir.epsilon_reduce || parsingResult.ActionDir == ActionDir.moveto)
                {
                    i--;
                }

                prevToken = token;
            }

            return result;
        }
    }
}
