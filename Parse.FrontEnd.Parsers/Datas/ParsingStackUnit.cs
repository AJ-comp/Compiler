using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.ParseTree;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ParsingStackUnit : ICloneable<ParsingStackUnit>
    {
        public Stack<object> Stack { get; } = new Stack<object>();
        public Stack<IEnumerable<AstSymbol>> AstListStack { get; } = new Stack<IEnumerable<AstSymbol>>();


        public ParsingStackUnit()
        {
        }

        public ParsingStackUnit(Stack<object> stack, Stack<IEnumerable<AstSymbol>> astListStack)
        {
            Stack = stack;
            AstListStack = astListStack;
        }

        public Tuple<ParseTreeSymbol, AstSymbol> Shift(TokenData seeingToken, object state)
        {
            AstSymbol astSymbol = null;
            var treeTerminal = new ParseTreeTerminal(seeingToken);

            Stack.Push(treeTerminal);
            Stack.Push(state);

            if (treeTerminal.IsMeaning)
            {
                astSymbol = new AstTerminal(seeingToken);
                AstListStack.Push(new List<AstSymbol>() { astSymbol });
            }

            return new Tuple<ParseTreeSymbol, AstSymbol>(treeTerminal, astSymbol);
        }

        public Tuple<ParseTreeSymbol, AstSymbol> Reduce(NonTerminalSingle reduceDest)
        {
            var dataToInsert = new ParseTreeNonTerminal(reduceDest);
            AstNonTerminal astNT = (dataToInsert.IsMeaning) ? new AstNonTerminal(dataToInsert)
                                                                                      : null;

            List <AstSymbol> astChildren = new List<AstSymbol>();
            for (int i = 0; i < reduceDest.Count * 2; i++)
            {
                var data = Stack.Pop();
                if (i % 2 > 0)
                {
                    var child = data as ParseTreeSymbol;
                    dataToInsert.Insert(0, child);

                    if (!child.IsMeaning) continue;
                    var astChild = AstListStack.Pop();
                    astChildren.AddRange(astChild);
                }
            }
            Stack.Push(dataToInsert);

            if (astNT != null)
            {
                astNT.AddRange(astChildren);
                AstListStack.Push(astNT);
            }
            else AstListStack.Push(astChildren);

            return new Tuple<ParseTreeSymbol, AstSymbol>(dataToInsert, astNT);
        }

        public Tuple<ParseTreeSymbol, AstSymbol> EpsilonReduce(NonTerminalSingle reduceDest)
        {
            var dataToInsert = new ParseTreeNonTerminal(reduceDest);
            AstNonTerminal astSymbol = (dataToInsert.IsMeaning) ? new AstNonTerminal(dataToInsert)
                                                                                    : null;

            Stack.Push(dataToInsert);
            if (astSymbol != null) AstListStack.Push(astSymbol);

            return new Tuple<ParseTreeSymbol, AstSymbol>(dataToInsert, astSymbol);
        }

        public void Goto(int state) => Stack.Push(state);

        public void Pop()
        {
            var obj = Stack.Pop();

            if (obj is ParseTreeSymbol)
            {
                if ((obj as ParseTreeSymbol).IsMeaning) AstListStack.Pop();
            }
        }

        public ParsingStackUnit Reverse() => new ParsingStackUnit(Stack.Reverse(), AstListStack.Reverse());

        public ParsingStackUnit Clone() => new ParsingStackUnit(Stack, AstListStack);
    }
}
