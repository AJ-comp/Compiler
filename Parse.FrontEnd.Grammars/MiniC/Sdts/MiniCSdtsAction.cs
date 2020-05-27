using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;
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
                    result.ConstToken = astNonTerminal.ActionLogic() as TokenData;
                }
                else if (astNonTerminal.SignPost.MeaningUnit == this.VoidNode)
                {
                    result.DataType = DataType.Void;
                    result.DataTypeToken = astNonTerminal.ActionLogic() as TokenData;
                }
                else if (astNonTerminal.SignPost.MeaningUnit == this.IntNode)
                {
                    result.DataType = DataType.Int;
                    result.DataTypeToken = astNonTerminal.ActionLogic() as TokenData;
                }
            }

            return result;
        }

        private object ActionTerminalNode(AstNonTerminal node) => (node.Items[0] as AstTerminal).Token;
        private object ActionConstNode(AstNonTerminal node) => ActionTerminalNode(node);
        private object ActionVoidNode(AstNonTerminal node) => ActionTerminalNode(node);
        private object ActionIntNode(AstNonTerminal node) => ActionTerminalNode(node);
        private object ActionVarNode(AstNonTerminal node) => ActionTerminalNode(node);
        private object ActionIntLiteralNode(AstNonTerminal node) => ActionTerminalNode(node);

        private object ActionFormalPara(AstNonTerminal node)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            foreach (var item in node.Items)
            {
                if (item is AstTerminal) continue;   // skip ( token and ) token

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

        private object ActionExpSt(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();
            if (curNode.Count == 0) return result;

            var astNonTerminal = curNode[0] as AstNonTerminal;
            result.AddRange(astNonTerminal.ActionLogic() as IReadOnlyList<AstSymbol>);
            result.Add(curNode);

            return result;
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

        private object ActionSimpleVar(AstNonTerminal curNode)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (curNode.Items[0] as AstTerminal).Token
            };

            return result;
        }

        // format summary
        // [0] : ident (AstTerminal)
        // [1] : number (AstTerminal)
        private object ActionArrayVar(AstNonTerminal curNode)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (curNode.Items[0] as AstTerminal).Token,
                DimensionToken = (curNode.Items[1] as AstTerminal).Token
            };

            return result;
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

        // format summary
        // [0] : ident (AstTerminal)
        // [1] : exp (AstNonTerminal) | number (AstTerminal)
        private object ActionIndex(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            if (curNode[1] is AstNonTerminal)
                result.AddRange((curNode[1] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);
            else result.Add(curNode[1]);

            result.Add(curNode[0]);
            result.Add(curNode);

            return result;
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
