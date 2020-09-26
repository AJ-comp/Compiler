using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        /// <summary>
        /// This function performs follow-up processing after shift operation.
        /// </summary>
        /// <param name="parsingBlock"></param>
        private void ShiftFollowUpProcess(ParsingUnit unitToParsing)
        {
            var afterStack = unitToParsing.AfterStack;

            var result = afterStack.Shift(unitToParsing.InputValue, unitToParsing.Action.Dest);

            // inform to external
            ParseTreeCreated?.Invoke(this, new ParseCreatedArgs(result.Item1, null));
            if (result.Item2 != null) ASTCreated?.Invoke(this, result.Item2);
        }

        /// <summary>
        /// This function performs follow-up processing after reduce operation.
        /// </summary>
        /// <param name="parsingUnit"></param>
        private void ReduceFollowUpProcess(ParsingUnit parsingUnit)
        {
            var reduceDest = parsingUnit.Action.Dest as NonTerminalSingle;
            var afterStack = parsingUnit.AfterStack;

            var result = afterStack.Reduce(reduceDest);

            // inform to external what parseTree, ast was created.
            ParseTreeCreated?.Invoke(this, new ParseCreatedArgs(result.Item1, null));
            if (result.Item2 != null) ASTCreated?.Invoke(this, result.Item2);
        }

        /// <summary>
        /// This function performs follow-up processing after epsilon reduce operation.
        /// </summary>
        /// <param name="parsingUnit"></param>
        private void EpsilonReduceFollowUpProcess(ParsingUnit parsingUnit)
        {
            var reduceDest = parsingUnit.Action.Dest as NonTerminalSingle;
            var afterStack = parsingUnit.AfterStack;

            var result = afterStack.EpsilonReduce(reduceDest);

            // inform to external what parseTree, ast was created.
            ParseTreeCreated?.Invoke(this, new ParseCreatedArgs(result.Item1, null));
            if (result.Item2 != null) ASTCreated?.Invoke(this, result.Item2);
        }

        /// <summary>
        /// This function performs follow-up processing after goto operation.
        /// </summary>
        /// <param name="parsingUnit"></param>
        private void GotoFollowUpProcess(ParsingUnit parsingUnit)
        {
            var reduceDest = parsingUnit.Action.Dest as NonTerminalSingle;
            var afterStack = parsingUnit.AfterStack;

            afterStack.Goto((int)parsingUnit.Action.Dest);
            GotoAction?.Invoke(this, parsingUnit);
        }

        /// <summary>
        /// This function builds a stack and parse tree information following action rule (parsingUnit.Action).
        /// </summary>
        /// <param name="unitToParsing"></param>
        protected void BuildStackAndParseTree(ParsingUnit unitToParsing)
        {
            // case shift
            if (unitToParsing.Action.Direction == ActionDir.shift) ShiftFollowUpProcess(unitToParsing);
            else if (unitToParsing.Action.Direction == ActionDir.reduce) ReduceFollowUpProcess(unitToParsing);
            else if (unitToParsing.Action.Direction == ActionDir.epsilon_reduce) EpsilonReduceFollowUpProcess(unitToParsing);
            else if (unitToParsing.Action.Direction == ActionDir.moveto) GotoFollowUpProcess(unitToParsing);
        }
    }
}
