using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private object ActionProgram(AstNonTerminal node)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as AstNonTerminal;

                if (astNonTerminal.SignPost.MeaningUnit == this.Dcl)
                    result.Add(this.ActionDcl(astNonTerminal) as AstSymbol);

                if (astNonTerminal.SignPost.MeaningUnit == this.FuncDef)
                    result.AddRange(this.ActionFuncDef(astNonTerminal) as IReadOnlyList<AstSymbol>);
            }

            return result;
        }

        // [0] : FuncHead (AstNonTerminal)
        // [1] : CompoundSt (AstNonTerminal)
        private object ActionFuncDef(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            var symbolTable = curNode.ConnectedSymbolTable as MiniCSymbolTable;
            var funcData = symbolTable.FuncDataList.ThisFuncData;
            if (funcData == null) return result;

            result.AddRange((curNode[0] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);
            result.AddRange((curNode[1] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);

            return result;
        }

        // [0] : DclSpec (AstNonTerminal)
        // [1] : Name (AstTerminal)
        // [2] : FormalPara (AstNonTerminal)
        private object ActionFuncHead(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>
            {
                curNode
            };

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.FormalPara)
                {
                    result.AddRange(this.ActionFormalPara(astNonterminal) as IReadOnlyList<AstSymbol>);
                }
            }

            return result;
        }

        private object ActionDclSpec(AstNonTerminal node)
        {
            DclSpecData result = new DclSpecData();

            foreach (var item in node.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == this.ConstNode)
                {
                    result.ConstToken = (astNonTerminal.Items[0] as AstTerminal).Token;
                }
                else if (astNonTerminal.SignPost.MeaningUnit == this.VoidNode)
                {
                    result.DataType = DataType.Void;
                    result.DataTypeToken = (astNonTerminal.Items[0] as AstTerminal).Token;
                }
                else if (astNonTerminal.SignPost.MeaningUnit == this.IntNode)
                {
                    result.DataType = DataType.Int;
                    result.DataTypeToken = (astNonTerminal.Items[0] as AstTerminal).Token;
                }
            }

            return result;
        }

        private object ActionFormalPara(AstNonTerminal node)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            foreach (var item in node.Items)
            {
                if (item is AstTerminal) continue;   // skip ( token and ) token

                VarData varData = new VarData();
                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.ParamDcl)
                {
                    result.Add(this.ActionParamDcl(astNonterminal) as AstSymbol);
                }
            }

            return result;
        }

        private object ActionParamDcl(AstNonTerminal curNode) => curNode;

        private object ActionCompoundSt(AstNonTerminal node)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            foreach (var item in node.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.DclList)
                    result.AddRange(this.ActionDclList(astNonterminal) as IReadOnlyList<AstSymbol>);
                else if (astNonterminal.SignPost.MeaningUnit == this.StatList)
                    result.AddRange(this.ActionStatList(astNonterminal) as IReadOnlyList<AstSymbol>);
            }

            return result;
        }

        private object ActionExpressionHub(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();
            if (curNode == null) return result;

            if (curNode.SignPost.MeaningUnit == this.Assign)
                result.AddRange(this.ActionAssign(curNode) as IReadOnlyList<AstSymbol>);
            else if (curNode.SignPost.MeaningUnit == this.AddAssign)
                result.AddRange(this.ActionAddAssign(curNode) as IReadOnlyList<AstSymbol>);
            else if (curNode.SignPost.MeaningUnit == this.SubAssign)
                result.AddRange(this.ActionSubAssign(curNode) as IReadOnlyList<AstSymbol>);
            else if (curNode.SignPost.MeaningUnit == this.MulAssign)
                result.AddRange(this.ActionMulAssign(curNode) as IReadOnlyList<AstSymbol>);
            else if (curNode.SignPost.MeaningUnit == this.DivAssign)
                result.AddRange(this.ActionDivAssign(curNode) as IReadOnlyList<AstSymbol>);
            else if (curNode.SignPost.MeaningUnit == this.ModAssign)
                result.AddRange(this.ActionModAssign(curNode) as IReadOnlyList<AstSymbol>);
            else if (curNode.SignPost.MeaningUnit == this.Call)
                result.AddRange(this.ActionCall(curNode) as IReadOnlyList<AstSymbol>);

            return result;
        }

        private object ActionExpSt(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();
            if (curNode.Count == 0) return result;

            var astNonTerminal = curNode[0] as AstNonTerminal;
            return ActionExpressionHub(astNonTerminal);
        }

        private object ActionDclList(AstNonTerminal node)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            foreach (var item in node.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.Dcl)
                    result.Add(this.ActionDcl(astNonterminal) as AstSymbol);
            }

            return result;
        }

        private object ActionDcl(AstNonTerminal curNode) => curNode;

        private object ActionDclItem(AstNonTerminal node)
        {
            DclItemData result = new DclItemData();

            foreach (var item in node.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == this.SimpleVar)
                    result = this.ActionSimpleVar(astNonTerminal) as DclItemData;
                else if (astNonTerminal.SignPost.MeaningUnit == this.ArrayVar)
                    result = this.ActionArrayVar(astNonTerminal) as DclItemData;
            }

            return result;
        }

        private object ActionSimpleVar(AstNonTerminal node)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (node.Items[0] as AstTerminal).Token
            };

            return result;
        }

        private object ActionArrayVar(AstNonTerminal node)
        {
            return null;
        }

        private object ActionStatList(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == this.ExpSt)
                    result.AddRange(this.ActionExpSt(astNonTerminal) as IReadOnlyList<AstSymbol>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.IfSt)
                    result.AddRange(this.ActionIfSt(astNonTerminal) as IReadOnlyList<AstSymbol>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.IfElseSt)
                    result.AddRange(this.ActionIfElseSt(astNonTerminal) as IReadOnlyList<AstSymbol>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.WhileSt)
                    result.AddRange(this.ActionWhileSt(astNonTerminal) as IReadOnlyList<AstSymbol>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.ReturnSt)
                    result.AddRange(this.ActionReturnSt(astNonTerminal) as IReadOnlyList<AstSymbol>);
            }

            return result;
        }


        // [0] : if (Terminal)
        // [1] : logical_exp (NonTerminal)
        // [2] : statement (NonTerminal)
        private object ActionIfSt(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            result.AddRange((curNode[1] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);
            result.Add(curNode);
            result.AddRange((curNode[2] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);

            return result;
        }

        private object ActionIfElseSt(AstNonTerminal node)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            return result;
        }

        private object ActionWhileSt(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            return result;
        }

        // [0] : return (AstTerminal)
        // [1] : ExpSt (AstNonTerminal)
        private object ActionReturnSt(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            result.AddRange((curNode[1] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);
            result.Add(curNode);

            return result;
        }

        private object ActionIndex(AstNonTerminal node)
        {
            return null;
        }

        // [0] : Ident (AstTerminal)
        // [1] : ActualParam (AstNonTerminal)
        private object ActionCall(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            result.AddRange((curNode[1] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);
            result.Add(curNode);

            return result;
        }

        private object ActionActualParam(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();
            if (curNode.Count == 0) return result;

            AstNonTerminal astNonTerminal = curNode[0] as AstNonTerminal;
            result.AddRange(astNonTerminal.ActionLogic() as IReadOnlyList<AstSymbol>);

            return result;
        }
    }
}
