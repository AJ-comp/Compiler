using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        /// <summary>
        /// 필요한 시점에 토큰 하나를 요청하여 파싱합니다. Reduce·Goto 동안 같은 룩어헤드를
        /// 보관하고, 실제 토큰을 Shift했을 때 입력 커서를 확정합니다. 현재 문맥을 source의 정책에
        /// 전달하고, 정책이 반환한 규칙 묶음으로 source에 다음 토큰을 요청합니다.
        /// EOF에서는 파싱 테이블의 Accept 액션을 확인합니다.
        /// 첫 오류에서 위치 진단을 반환하며, 기존 목록 입력의 사용자 정의 복구는 별도 경로입니다.
        /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#parser-role">파서 주도 렉싱의 호출 순서와 이유</see></para>
        /// </summary>
        /// <exception cref="ArgumentNullException">source가 null인 경우입니다.</exception>
        /// <exception cref="NotSupportedException">사용자 정의 ErrorHandler 또는
        /// BackTrackingOnConflict가 설정된 경우입니다. 이 설정에는 토큰 목록 입력 API를 사용합니다.</exception>
        public ParsingResult Parsing(ITokenSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            // Recovery rewrites logical token lists. Reject an incompatible configuration
            // explicitly instead of silently ignoring an installed recovery/backtracking policy.
            if (ErrorHandler != null || BackTrackingOnConflict)
                throw new NotSupportedException("Token-source parsing stops at the first error. Custom recovery and general backtracking use the token-list parsing API.");

            FiredException = null;
            _predicateException = null;
            var result = new ParsingResult(_logging);
            var table = (LRParsingTable)ParsingTable;
            ContextualInput input = null;
            ParsingBlock block = null;
            ParsingStackUnit lastStack = null;
            try
            {
                input = new ContextualInput(source);
                while (true)
                {
                    // Only reaching this point after a Shift requests another lexical token.
                    var states = lastStack == null ? new[] { 0 } : States(lastStack);
                    var token = input.Read(states, table);
                    result.AddNewBlock(token);
                    block = result[result.Count - 1];
                    block.AddItem(lastStack);
                    var reductions = new HashSet<string>(StringComparer.Ordinal);
                    int zeroWidthSteps = 0;

                    while (true)
                    {
                        var unit = block.Units.Last();
                        // No token can make progress inside this loop until Shift. Detect
                        // a repeated LR configuration, including the temporary pre-Goto NT.
                        var key = StateKey(unit.BeforeStack);
                        if (!reductions.Add(key))
                            throw new ContextualLexingException("A non-consuming parsing cycle was detected",
                                States(unit.BeforeStack).Last(), input.Cursor.SourceOffset, input.Cursor.TokenIndex);
                        if (++zeroWidthSteps > 1000000 || unit.BeforeStack.Stack.Count > 32769)
                            throw new ContextualLexingException("Non-consuming parsing exceeded its work or stack limit",
                                States(unit.BeforeStack).Last(), input.Cursor.SourceOffset, input.Cursor.TokenIndex);

                        var step = UnitParsing(unit, token, null, input.Cursor.TokenIndex, input);
                        if (step.Item1 == SuccessedKind.ReduceOrGoto)
                        {
                            block.AddItem(unit.AfterStack);
                            continue;
                        }
                        if (step.Item1 == SuccessedKind.Shift)
                        {
                            if (token.Kind is EndMarker)
                                throw new ContextualLexingException("EOF must be accepted, never shifted",
                                    States(unit.BeforeStack).Last(), input.Cursor.SourceOffset, input.Cursor.TokenIndex);
                            input.Shift();
                            lastStack = unit.AfterStack;
                            block = null;
                            break;
                        }
                        if (step.Item1 == SuccessedKind.Completed)
                        {
                            if (!(token.Kind is EndMarker))
                                throw new ContextualLexingException("Accept requires the end of the source",
                                    States(unit.BeforeStack).Last(), input.Cursor.SourceOffset, input.Cursor.TokenIndex);
                            result.Success = true;
                            return result;
                        }

                        ActionFailed?.Invoke(this, unit);
                        RecordTokenSourceError(block, _predicateException ?? new ParsingException(input.Cursor.TokenIndex), token);
                        FiredException = _predicateException ?? new ParsingException(input.Cursor.TokenIndex);
                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                FiredException = exception;
                var offset = exception is ContextualLexingException lexicalError
                    ? lexicalError.SourceOffset : input?.Cursor.SourceOffset ?? 0;
                // A zero-width positioned marker reports an error before a token could be
                // classified without inventing a source character or blaming the last Shift.
                var marker = input?.Pending?.Token ?? new TokenData(new NotDefined(),
                    new TokenCell(offset, -1, -1, string.Empty, null));
                if (block == null)
                {
                    result.AddNewBlock(marker);
                    block = result[result.Count - 1];
                    block.AddItem(lastStack);
                }
                var failed = block.Units.Last();
                failed.InputValue = marker;
                failed.ChangeToFailedState(exception.Message);
                failed.Action = new ActionData(ActionDir.Failed, null);
                RecordTokenSourceError(block, exception, marker);
                return result;
            }
        }

        private static int[] States(ParsingStackUnit stack)
            => stack.Stack.OfType<int>().Reverse().ToArray();

        private static string StateKey(ParsingStackUnit stack)
            => string.Join(",", States(stack)) + (stack.Stack.Peek() is ParseTreeNonTerminal nonTerminal
                ? "|goto:" + nonTerminal.ToNonTerminal.UniqueKey : "|action");

        private static void RecordTokenSourceError(ParsingBlock block, Exception exception, TokenData token)
        {
            if (exception is GrammarPredicateEvaluationException predicateError)
                token = predicateError.Context.NextToken;
            else if (exception is ContextualLexingException lexicalError)
                token = new TokenData(new NotDefined(), new TokenCell(lexicalError.SourceOffset, -1, -1, string.Empty, null));
            else if (exception is PredictionException predictionError && predictionError.SourceOffset.HasValue)
                token = new TokenData(new NotDefined(), new TokenCell(predictionError.SourceOffset.Value, -1, -1, string.Empty, null));
            var reason = block.Units.LastOrDefault()?.ErrorMessage;
            block._errorInfos.Add(ParsingErrorInfo.CreateParsingError(token, "JLLEX",
                string.IsNullOrEmpty(reason) ? exception.Message : reason));
        }
    }
}
