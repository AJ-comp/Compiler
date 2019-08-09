using Parse.Ast;
using System;
using System.Collections.Generic;

namespace Parse.RegularGrammar
{
    public class Logic
    {
        public enum MeaningUnit
        {
            Empty,
            Program,
            FuncDef,
            FuncHead,
            DclSpec,
            Add, Sub, Mul, Div, Assign, AddAssign, SubAssign, MulAssign, DivAssign,   // calculate
            IfStatement, IfElseStatement,                   // control
            WhileStatement, ForStatement,                 // loof
        }

        public Action<AstSymbol, AstSymbol> AddAction = null;
        public Action<AstSymbol, AstSymbol> SubAction = null;
        public Action<AstSymbol, AstSymbol> MulAction = null;
        public Action<AstSymbol, AstSymbol> DivAction = null;
        public Action<AstSymbol, AstSymbol> AssignAction = null;


        public void SignPost(MeaningUnit meaningUnit, List<AstSymbol> symbols)
        {
            if (meaningUnit == MeaningUnit.Add) this.AddAction?.Invoke(symbols[0], symbols[1]);
            else if (meaningUnit == MeaningUnit.Sub) this.SubAction?.Invoke(symbols[0], symbols[1]);
        }
    }
}
