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
        private object ActionProgram(TreeNonTerminal node, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            // it is can parsed only perfect tree.
            if (node.HasVirtualChild) return result;

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.Dcl)
                    result.Add(this.ActionDcl(astNonTerminal, blockLevel, 0) as TreeNonTerminal);

                if (astNonTerminal._signPost.MeaningUnit == this.FuncDef)
                    result.AddRange(this.ActionFuncDef(astNonTerminal, blockLevel, 0) as IReadOnlyList<TreeNonTerminal>);
            }

            return result;
        }

        private object ActionFuncDef(TreeNonTerminal node, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            var symbolTable = node.ConnectedSymbolTable as MiniCSymbolTable;
            var funcData = symbolTable.FuncDataList.ThisFuncData;
            if (funcData == null) return result;
            offset = funcData.ParamVars.Count;

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.FuncHead)
                    result.AddRange(this.ActionFuncHead(astNonTerminal, blockLevel, offset) as IReadOnlyList<TreeNonTerminal>);
                else if (astNonTerminal._signPost.MeaningUnit == this.CompoundSt)
                    result.AddRange(this.ActionCompoundSt(astNonTerminal, blockLevel + 1, offset) as IReadOnlyList<TreeNonTerminal>);
            }

            return result;
        }

        private object ActionFuncHead(TreeNonTerminal curNode, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>
            {
                curNode
            };

            foreach (var item in curNode.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.FormalPara)
                {
                    result.AddRange(this.ActionFormalPara(astNonterminal, blockLevel, offset) as IReadOnlyList<TreeNonTerminal>);
                }
            }

            return result;
        }

        private object ActionDclSpec(TreeNonTerminal node, int blockLevel, int offset)
        {
            DclSpecData result = new DclSpecData();

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.ConstNode)
                {
                    result.ConstToken = (astNonTerminal.Items[0] as TreeTerminal).Token;
                }
                else if (astNonTerminal._signPost.MeaningUnit == this.VoidNode)
                {
                    result.DataType = DataType.Void;
                    result.DataTypeToken = (astNonTerminal.Items[0] as TreeTerminal).Token;
                }
                else if (astNonTerminal._signPost.MeaningUnit == this.IntNode)
                {
                    result.DataType = DataType.Int;
                    result.DataTypeToken = (astNonTerminal.Items[0] as TreeTerminal).Token;
                }
            }

            return result;
        }

        private object ActionFormalPara(TreeNonTerminal node, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;   // skip ( token and ) token

                VarData varData = new VarData();
                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.ParamDcl)
                {
                    result.Add(this.ActionParamDcl(astNonterminal, blockLevel, offset) as TreeNonTerminal);
                }
            }

            return result;
        }

        private object ActionParamDcl(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionCompoundSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal) continue;

                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.DclList)
                    result.AddRange(this.ActionDclList(astNonterminal, blockLevel, offset) as IReadOnlyList<TreeNonTerminal>);
                else if (astNonterminal._signPost.MeaningUnit == this.StatList)
                    result.AddRange(this.ActionStatList(astNonterminal, blockLevel, offset) as IReadOnlyList<TreeNonTerminal>);
            }

            return result;
        }

        private object ActionExpSt(TreeNonTerminal curNode, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            foreach (var item in curNode.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.AddAssign)
                    result.Add(this.ActionAddAssign(astNonTerminal, blockLevel, offset) as TreeNonTerminal);
            }

            return result;
        }

        private object ActionDclList(TreeNonTerminal node, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal) continue;

                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.Dcl)
                    result.Add(this.ActionDcl(astNonterminal, blockLevel, offset) as TreeNonTerminal);
            }

            return result;
        }

        private object ActionDcl(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionDclItem(TreeNonTerminal node, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData();

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.SimpleVar)
                    result = this.ActionSimpleVar(astNonTerminal, blockLevel, offset) as DclItemData;
                else if (astNonTerminal._signPost.MeaningUnit == this.ArrayVar)
                    result = this.ActionArrayVar(astNonTerminal, blockLevel, offset) as DclItemData;
            }

            return result;
        }

        private object ActionSimpleVar(TreeNonTerminal node, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (node.Items[0] as TreeTerminal).Token
            };

            return result;
        }

        private object ActionArrayVar(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionStatList(TreeNonTerminal node, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.IfSt)
                    result.AddRange(this.ActionIfSt(astNonTerminal, blockLevel, offset) as IReadOnlyList<TreeNonTerminal>);
                else if (astNonTerminal._signPost.MeaningUnit == this.IfElseSt)
                    result.AddRange(this.ActionIfElseSt(astNonTerminal, blockLevel, offset) as IReadOnlyList<TreeNonTerminal>);
                else if (astNonTerminal._signPost.MeaningUnit == this.WhileSt)
                    result.AddRange(this.ActionWhileSt(astNonTerminal, blockLevel, offset) as IReadOnlyList<TreeNonTerminal>);
                else if (astNonTerminal._signPost.MeaningUnit == this.ExpSt)
                    result.AddRange(this.ActionExpSt(astNonTerminal, blockLevel, offset) as IReadOnlyList<TreeNonTerminal>);
            }

            return result;
        }

        private object ActionIfSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionIfElseSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionWhileSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionReturnSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionIndex(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionCell(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionActualParam(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionAdd(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionSub(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionMul(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionDiv(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionMod(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionAssign(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionAddAssign(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionSubAssign(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionMulAssign(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionDivAssign(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionModAssign(TreeNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionLogicalOr(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionLogicalAnd(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionLogicalNot(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionEqual(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionNotEqual(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionGreaterThan(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionLessThan(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionGreatherEqual(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionLessEqual(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionUnaryMinus(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionPreInc(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionPreDec(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionPostInc(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionPostDec(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }


        public override SementicAnalysisResult Process(TreeSymbol symbol)
        {
            MeaningErrInfoList errList = new MeaningErrInfoList();

            if (symbol != null)
            {
                this.BuildProgramNode(symbol as TreeNonTerminal, new MiniCSymbolTable(), 0, 0);
                return new SementicAnalysisResult(errList, null);
            }
            else return null;
        }

        public override IReadOnlyList<TreeNonTerminal> GenerateCode(TreeSymbol symbol)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            if (symbol != null)
            {
                result = this.ActionProgram(symbol as TreeNonTerminal, 0, 0) as List<TreeNonTerminal>;
            }

            return result;
        }
    }
}
