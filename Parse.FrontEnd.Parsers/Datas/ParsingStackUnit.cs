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
        public Stack<object> AstListStack { get; } = new Stack<object>();


        public ParsingStackUnit()
        {
        }

        public ParsingStackUnit(Stack<object> stack, Stack<object> astListStack)
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
                AstListStack.Push(astSymbol);
            }

            return new Tuple<ParseTreeSymbol, AstSymbol>(treeTerminal, astSymbol);
        }

        public Tuple<ParseTreeSymbol, AstSymbol> Reduce(NonTerminalSingle reduceDest)
        {
            var dataToInsert = new ParseTreeNonTerminal(reduceDest);
            AstNonTerminal astNT = (dataToInsert.IsMeaning) ? new AstNonTerminal(dataToInsert)
                                                                                      : null;

            List<object> astChildren = new List<object>();
            for (int i = 0; i < reduceDest.Count * 2; i++)
            {
                var data = Stack.Pop();
                if (i % 2 > 0)
                {
                    var child = data as ParseTreeSymbol;
                    dataToInsert.Insert(0, child);

                    if (!child.IsMeaning && child.IsTerminal) continue;
                    if (child is ParseTreeNonTerminal)
                    {
                        // epsilon reduce case
                        if ((child as ParseTreeNonTerminal).Count == 0) continue;
                    }

                    var astChild = AstListStack.Pop();
                    astChildren.Insert(0, astChild);
                }
            }
            Stack.Push(dataToInsert);

            if (astNT != null)
            {
                astNT.AddRange(TakeOffList(astChildren));
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

        public ParsingStackUnit Clone() => new ParsingStackUnit(Stack.Clone(), AstListStack.Clone());


        private IEnumerable<AstSymbol> TakeOffList(IEnumerable<object> list)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            foreach (var item in list)
            {
                if (item is AstSymbol) result.Add(item as AstSymbol);
                else result.AddRange(TakeOffList(item as List<object>));
            }

            return result;
        }
    }
}
