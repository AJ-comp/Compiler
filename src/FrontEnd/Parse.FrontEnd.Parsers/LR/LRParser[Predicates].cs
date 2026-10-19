using System;
using System.Collections;
using System.Collections.Generic;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.RegularGrammar;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        private static GrammarPredicate GetPredicate(ActionData action)
            => action?.Direction == ActionDir.EpsilonReduce && action.Dest is NonTerminalSingle production
                ? production.Predicate : null;

        // Prediction and actual execution use exactly the same callback and token context.
        // Reductions consume no token, so their boundary is the next token's physical index.
        private bool EvaluatePredicate(ActionData action, IReadOnlyList<TokenData> tokens, int index)
            => EvaluatePredicate(action, CreatePredicateContext(tokens, index));

        private static bool EvaluatePredicate(ActionData action, PredicateContext context)
        {
            var predicate = GetPredicate(action);
            if (predicate == null) return true;
            try
            {
                return predicate.Evaluate(context);
            }
            catch (Exception exception)
            {
                throw new GrammarPredicateEvaluationException(predicate.Name, context, exception);
            }
        }

        private static PredicateContext CreatePredicateContext(IReadOnlyList<TokenData> tokens, int index)
            => new PredicateContext(index == 0 ? null : tokens[index - 1], tokens[index], index);

        private static IReadOnlyList<TokenData> GetPredicateTokens(ParsingResult result)
            => result.PredicateTokens ?? new ParsingTokenView(result);

        private static int GetPredicateTokenIndex(ParsingResult result, int blockIndex)
            => result.PredicateTokenIndices == null ? blockIndex : result.PredicateTokenIndices[blockIndex];

        private void RecordPredicateFailure(ParsingUnit unit, Exception exception)
        {
            unit.PredicateException = exception;
            _predicateException = exception;
            unit.ChangeToFailedState(exception.Message);
        }

        private void CheckSelectedPredicate(ParsingUnit unit)
        {
            var predicate = GetPredicate(unit.Action);
            if (predicate == null || unit.IsError) return;
            var context = unit.PredicateContext;
            if (context == null || !ReferenceEquals(context.NextToken, unit.InputValue))
            {
                // Isolated UnitParsing / explicitly supplied recovery actions may have no
                // surrounding input. Make that absence explicit rather than inventing a token.
                context = new PredicateContext(null, unit.InputValue, -1);
                unit.PredicateContext = context;
                unit.PredicateCheckedAction = null;
            }
            if (ReferenceEquals(unit.PredicateCheckedAction, unit.Action)) return;

            try
            {
                if (!EvaluatePredicate(unit.Action, context))
                    unit.ChangeToFailedState($"Grammar predicate {predicate.Name} evaluated to false.");
                else unit.PredicateCheckedAction = unit.Action;
            }
            catch (GrammarPredicateEvaluationException exception)
            {
                RecordPredicateFailure(unit, exception);
            }
        }

        // The result already owns the token blocks. This O(1) indexed view avoids repeatedly
        // copying the complete input for each unit, and reflects recovery block replacements.
        private sealed class ParsingTokenView : IReadOnlyList<TokenData>
        {
            private readonly ParsingResult _result;
            public ParsingTokenView(ParsingResult result) { _result = result; }
            public int Count => _result.Count;
            public TokenData this[int index] => _result[index].Token;
            public IEnumerator<TokenData> GetEnumerator()
            {
                for (int i = 0; i < Count; i++) yield return this[i];
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
