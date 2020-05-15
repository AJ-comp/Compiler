using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;
using System;
using System.Collections.Generic;
using AlarmCodes = Parse.FrontEnd.Grammars.Properties.AlarmCodes;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private object ActionProgram(AstNonTerminal node, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as AstNonTerminal;

                if (astNonTerminal.SignPost.MeaningUnit == this.Dcl)
                    result.Add(this.ActionDcl(astNonTerminal, blockLevel, 0) as AstNonTerminal);

                if (astNonTerminal.SignPost.MeaningUnit == this.FuncDef)
                    result.AddRange(this.ActionFuncDef(astNonTerminal, blockLevel, 0) as IReadOnlyList<AstNonTerminal>);
            }

            return result;
        }

        private object ActionFuncDef(AstNonTerminal node, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            var symbolTable = node.ConnectedSymbolTable as MiniCSymbolTable;
            var funcData = symbolTable.FuncDataList.ThisFuncData;
            if (funcData == null) return result;
            offset = funcData.ParamVars.Count;

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as AstNonTerminal;

                if (astNonTerminal.SignPost.MeaningUnit == this.FuncHead)
                    result.AddRange(this.ActionFuncHead(astNonTerminal, blockLevel, offset) as IReadOnlyList<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.CompoundSt)
                    result.AddRange(this.ActionCompoundSt(astNonTerminal, blockLevel + 1, offset) as IReadOnlyList<AstNonTerminal>);
            }

            return result;
        }

        private object ActionFuncHead(AstNonTerminal curNode, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>
            {
                curNode
            };

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.FormalPara)
                {
                    result.AddRange(this.ActionFormalPara(astNonterminal, blockLevel, offset) as IReadOnlyList<AstNonTerminal>);
                }
            }

            return result;
        }

        private object ActionDclSpec(AstNonTerminal node, int blockLevel, int offset)
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

        private object ActionFormalPara(AstNonTerminal node, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            foreach (var item in node.Items)
            {
                if (item is AstTerminal) continue;   // skip ( token and ) token

                VarData varData = new VarData();
                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.ParamDcl)
                {
                    result.Add(this.ActionParamDcl(astNonterminal, blockLevel, offset) as AstNonTerminal);
                }
            }

            return result;
        }

        private object ActionParamDcl(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionCompoundSt(AstNonTerminal node, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            foreach (var item in node.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.DclList)
                    result.AddRange(this.ActionDclList(astNonterminal, blockLevel, offset) as IReadOnlyList<AstNonTerminal>);
                else if (astNonterminal.SignPost.MeaningUnit == this.StatList)
                    result.AddRange(this.ActionStatList(astNonterminal, blockLevel, offset) as IReadOnlyList<AstNonTerminal>);
            }

            return result;
        }

        private object ActionExpSt(AstNonTerminal curNode, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == this.Assign)
                    result.AddRange(this.ActionAssign(astNonTerminal, blockLevel, offset) as List<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.AddAssign)
                    result.AddRange(this.ActionAddAssign(astNonTerminal, blockLevel, offset) as List<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.SubAssign)
                    result.AddRange(this.ActionSubAssign(astNonTerminal, blockLevel, offset) as List<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.MulAssign)
                    result.AddRange(this.ActionMulAssign(astNonTerminal, blockLevel, offset) as List<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.DivAssign)
                    result.AddRange(this.ActionDivAssign(astNonTerminal, blockLevel, offset) as List<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.ModAssign)
                    result.AddRange(this.ActionModAssign(astNonTerminal, blockLevel, offset) as List<AstNonTerminal>);

            }

            return result;
        }

        private object ActionDclList(AstNonTerminal node, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            foreach (var item in node.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.Dcl)
                    result.Add(this.ActionDcl(astNonterminal, blockLevel, offset) as AstNonTerminal);
            }

            return result;
        }

        private object ActionDcl(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionDclItem(AstNonTerminal node, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData();

            foreach (var item in node.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == this.SimpleVar)
                    result = this.ActionSimpleVar(astNonTerminal, blockLevel, offset) as DclItemData;
                else if (astNonTerminal.SignPost.MeaningUnit == this.ArrayVar)
                    result = this.ActionArrayVar(astNonTerminal, blockLevel, offset) as DclItemData;
            }

            return result;
        }

        private object ActionSimpleVar(AstNonTerminal node, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (node.Items[0] as AstTerminal).Token
            };

            return result;
        }

        private object ActionArrayVar(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionStatList(AstNonTerminal node, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            foreach (var item in node.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == this.IfSt)
                    result.AddRange(this.ActionIfSt(astNonTerminal, blockLevel, offset) as IReadOnlyList<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.IfElseSt)
                    result.AddRange(this.ActionIfElseSt(astNonTerminal, blockLevel, offset) as IReadOnlyList<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.WhileSt)
                    result.AddRange(this.ActionWhileSt(astNonTerminal, blockLevel, offset) as IReadOnlyList<AstNonTerminal>);
                else if (astNonTerminal.SignPost.MeaningUnit == this.ExpSt)
                    result.AddRange(this.ActionExpSt(astNonTerminal, blockLevel, offset) as IReadOnlyList<AstNonTerminal>);
            }

            return result;
        }

        private object ActionIfSt(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionIfElseSt(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionWhileSt(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionReturnSt(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionIndex(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionCell(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionActualParam(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }
    }
}
