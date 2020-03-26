using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ParseTreeBuilder
    {
        /*
        /// <summary>
        /// This function builds a parse tree.
        /// </summary>
        /// <param name="successResult">The result when the 1 level parsing is a success</param>
        private static void BuildParseTree(ParsingUnit successResult)
        {
            if (successResult.Action.Direction == ActionDir.shift)
            {
                //                if (!args.InputValue.Kind.Meaning) return;

                successResult.ParseTreeStack.Push(new TreeTerminal(successResult.InputValue));
            }
            else if (successResult.Action.Direction == ActionDir.reduce)
            {
                var item = successResult.Action.Dest as NonTerminalSingle;

                TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                for (int i = 0; i < item.Count; i++) nonTerminal.Insert(0, successResult.ParseTreeStack.Pop());

                successResult.ParseTreeStack.Push(nonTerminal);
            }
            else if (successResult.Action.Direction == ActionDir.epsilon_reduce)
            {
                var item = successResult.Action.Dest as NonTerminalSingle;
                TreeNonTerminal nonTerminal = new TreeNonTerminal(item);

                successResult.ParseTreeStack.Push(nonTerminal);
            }
        }
        */


        /*
        /// <summary>
        /// This function builds an AST
        /// </summary>
        /// <param name="successResult">The result when the 1 level parsing is a success</param>
        private void BuildAST(ParsingUnit successResult)
        {
            if (successResult.Action.Direction == ActionDir.shift)
            {
                if (!successResult.InputValue.Kind.Meaning) return;

                this.data.Push(new TreeTerminal(successResult.InputValue));
            }
            else if (successResult.Action.Direction == ActionDir.reduce)
            {
                var item = successResult.Action.Dest as NonTerminalSingle;

                if (item.MeaningUnit != null)
                {
                    TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                    for (int i = 0; i < item.Count; i++)
                    {
                        var symbol = this.data.Pop();
                        if (symbol is TreeNonTerminal)
                        {
                            var nonTerminalSymbol = symbol as TreeNonTerminal;
                            if (nonTerminalSymbol.Items.Count == 0)  // epsilon
                                continue;
                        }
                        nonTerminal.Insert(0, symbol);
                    }
                    this.data.Push(nonTerminal);
                }
            }
            else if (successResult.Action.Direction == ActionDir.epsilon_reduce)
            {
                var item = successResult.Action.Dest as NonTerminalSingle;

                //                if (item.MeaningUnit != null)
                //                {
                TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                this.data.Push(nonTerminal);
                //                }
            }
        }
        */

        /// <summary>
        /// This function builds a ParseTree and an AST.
        /// </summary>
        /// <param name="successResult">The result when the 1 level parsing is a success</param>
        public static void BuildTree(ParsingUnit successResult)
        {
//            ParseTreeBuilder.BuildParseTree(successResult);
//            this.BuildAST(successResult);
        }
    }
}
