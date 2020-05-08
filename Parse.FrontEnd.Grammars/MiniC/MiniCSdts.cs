using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class MiniCSdts : Sdts
    {
        // The cache role for speed up
        private MiniCGrammar grammar;

        public override event EventHandler<SementicErrorArgs> SementicErrorEventHandler;

        public MeaningUnit Program { get; } = new MeaningUnit("Program");
        public MeaningUnit FuncDef { get; } = new MeaningUnit("FuncDef");
        public MeaningUnit FuncHead { get; } = new MeaningUnit("FuncHead");
        public MeaningUnit DclSpec { get; } = new MeaningUnit("DclSpec");
        public MeaningUnit ConstNode { get; } = new MeaningUnit("ConstNode");
        public MeaningUnit IntNode { get; } = new MeaningUnit("IntNode");
        public MeaningUnit VoidNode { get; } = new MeaningUnit("VoidNode");
        public MeaningUnit FormalPara { get; } = new MeaningUnit("FormalPara");
        public MeaningUnit ParamDcl { get; } = new MeaningUnit("ParamDcl");
        public MeaningUnit CompoundSt { get; } = new MeaningUnit("CompoundSt");
        public MeaningUnit DclList { get; } = new MeaningUnit("DclList");
        public MeaningUnit Dcl { get; } = new MeaningUnit("Dcl");
        public MeaningUnit DclItem { get; } = new MeaningUnit("DclItem");
        public MeaningUnit SimpleVar { get; } = new MeaningUnit("SimpleVar");
        public MeaningUnit ArrayVar { get; } = new MeaningUnit("ArrayVar");
        public MeaningUnit StatList { get; } = new MeaningUnit("StatList");
        public MeaningUnit ExpSt { get; } = new MeaningUnit("ExpSt");
        public MeaningUnit IfSt { get; } = new MeaningUnit("IfSt");
        public MeaningUnit IfElseSt { get; } = new MeaningUnit("IfElseSt");
        public MeaningUnit WhileSt { get; } = new MeaningUnit("WhileSt");
        public MeaningUnit ReturnSt { get; } = new MeaningUnit("ReturnSt");
        public MeaningUnit Index { get; } = new MeaningUnit("Index");
        public MeaningUnit Cell { get; } = new MeaningUnit("Cell");
        public MeaningUnit ActualParam { get; } = new MeaningUnit("ActualParam");


        private object ActionProgram(TreeNonTerminal node, int blockLevel, int offset)
        {
            string result = string.Empty;

            // it is can parsed only perfect tree.
            if (node.HasVirtualChild) return result;

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.Dcl)
                    result += this.ActionDcl(astNonTerminal, blockLevel, offset++);
                else if(astNonTerminal._signPost.MeaningUnit == this.FuncDef)
                    result += this.ActionFuncDef(astNonTerminal, blockLevel, 0);
            }

            return result;
        }

        private object CheckProgramNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            foreach (var item in node.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.Dcl)
                {
                    var nodeCheckResult = this.CheckDclNode(astNonTerminal, baseSymbolTable, blockLevel, offset++);
                    baseSymbolTable = nodeCheckResult.symbolTable as MiniCSymbolTable;
                }
                else if (astNonTerminal._signPost.MeaningUnit == this.FuncDef)
                {
                    this.CheckFuncDefNode(astNonTerminal, baseSymbolTable, blockLevel, 0);
                }
            }

            return null;
        }

        private object ActionFuncDef(TreeNonTerminal node, int blockLevel, int offset)
        {
            string result = string.Empty;
            string compoundSt = string.Empty;

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.CompoundSt)
                    compoundSt += this.ActionCompoundSt(item as TreeNonTerminal, blockLevel + 1, offset);
            }

            var symbolTable = node.ConnectedSymbolTable as MiniCSymbolTable;
            var funcData = symbolTable.FuncDataList.ThisFuncData;
            if (funcData == null) return result;

            result += UCode.ProcStart(funcData.Name, 0, blockLevel, funcData.Name + " function");
            result += compoundSt;
            result += UCode.ProcEnd();

            return result;
        }

        private NodeCheckResult CheckFuncDefNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            var newSymbolTable = new MiniCSymbolTable(baseSymbolTable);

            foreach (var item in curNode.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.FuncHead)
                {
                    var nodeCheckResult = this.CheckFuncHeadNode(item as TreeNonTerminal, baseSymbolTable, blockLevel + 1, offset);
                    var funcData = nodeCheckResult.Data as FuncData;
                    offset = funcData.LocalVars.Count;

                    funcData.This = true;
                    baseSymbolTable.FuncDataList.Add(funcData);
                    baseSymbolTable.VarDataList.AddRange(funcData.LocalVars);

                    curNode.ConnectedSymbolTable = baseSymbolTable;
                }
                else if (astNonTerminal._signPost.MeaningUnit == this.CompoundSt)
                {
                    this.CheckCompoundStNode(item as TreeNonTerminal, baseSymbolTable, blockLevel + 1, offset);
                }
            }

            return new NodeCheckResult(null, baseSymbolTable);
        }

        /// <summary>
        /// FuncHead
        ///     DclSpec
        ///     ident
        ///     FormalParam
        /// </summary>
        /// <param name="node"></param>
        /// <param name="table"></param>
        /// <param name="parsingInfo"></param>
        /// <param name="errList"></param>
        /// <returns></returns>
        private object ActionFuncHead(TreeNonTerminal node, int blockLevel, int offset)
        {
            FuncData result = new FuncData();

            foreach(var item in node.Items)
            {
                // ident
                if(item is TreeTerminal)
                {
                    var token = item as TreeTerminal;

                    result.NameToken = token.Token;
                }
                else
                {
                    var astNonterminal = item as TreeNonTerminal;
                    if (astNonterminal._signPost.MeaningUnit == this.DclSpec)
                        result.DclSpecData = this.ActionDclSpec(astNonterminal, blockLevel, offset) as DclSpecData;
                    else if (astNonterminal._signPost.MeaningUnit == this.FormalPara)
                    {
                        var varDatas = this.ActionFormalPara(astNonterminal, blockLevel, offset) as List<VarData>;
                        result.LocalVars.AddRange(varDatas);
                    }
                }
            }

            return result;
        }

        private NodeCheckResult CheckFuncHeadNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            FuncData result = new FuncData();

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal)
                {
                    var token = item as TreeTerminal;

                    result.NameToken = token.Token;
                }
                else
                {
                    var astNonterminal = item as TreeNonTerminal;
                    if (astNonterminal._signPost.MeaningUnit == this.DclSpec)
                    {
                        var nodeCheckResult = this.CheckDclSpecNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                        result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                    }
                    else if (astNonterminal._signPost.MeaningUnit == this.FormalPara)
                    {
                        var nodeCheckResult = this.CheckFormalParaNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                        var datas = nodeCheckResult.Data as List<VarData>;
                        result.LocalVars.AddRange(datas);
                    }
                }
            }

            return new NodeCheckResult(result, null);
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

        private NodeCheckResult CheckDclSpecNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
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

            return new NodeCheckResult(result, baseSymbolTable);
        }

        private object ActionFormalPara(TreeNonTerminal node, int blockLevel, int offset)
        {
            List<VarData> result = new List<VarData>();

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;   // skip ( token and ) token

                VarData varData = new VarData();
                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.ParamDcl)
                {
                    varData.DclData = this.ActionParamDcl(astNonterminal, blockLevel, offset) as DclData;
                    result.Add(varData);
                }
            }

            return result;
        }

        private NodeCheckResult CheckFormalParaNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            List<VarData> result = new List<VarData>();

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;   // skip ( token and ) token

                VarData varData = new VarData();
                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.ParamDcl)
                {
                    var nodeCheckResult = this.CheckParamDcl(astNonterminal, baseSymbolTable, blockLevel, offset);
                    varData.DclData = nodeCheckResult.Data as DclData;
                    result.Add(varData);
                }
            }

            return new NodeCheckResult(result, baseSymbolTable);
        }

        private object ActionParamDcl(TreeNonTerminal node, int blockLevel, int offset)
        {
            DclData result = new DclData();
            result.BlockLevel = blockLevel;

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal)
                {
                    var token = item as TreeTerminal;

//                    result. = token.Token.Input;
//                    result.NameToken = token.Token;
                    //                    token.Token.Input
                }
                else
                {
                    var astNonterminal = item as TreeNonTerminal;
                    if (astNonterminal._signPost.MeaningUnit == this.DclSpec)
                        result.DclSpecData = this.ActionDclSpec(astNonterminal, blockLevel, offset) as DclSpecData;
                }
            }

            return result;
        }

        private NodeCheckResult CheckParamDcl(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclData result = new DclData();
            result.BlockLevel = blockLevel;

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal)
                {
                    var token = item as TreeTerminal;

                    //                    result. = token.Token.Input;
                    //                    result.NameToken = token.Token;
                    //                    token.Token.Input
                }
                else
                {
                    var astNonterminal = item as TreeNonTerminal;
                    if (astNonterminal._signPost.MeaningUnit == this.DclSpec)
                    {
                        var nodeCheckResult = this.CheckDclSpecNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                        result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                    }
                    else if (astNonterminal._signPost.MeaningUnit == this.SimpleVar)
                    {
                        var nodeCheckResult = this.CheckSimpleVarNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                        result.DclItemData = nodeCheckResult.Data as DclItemData;
                    }
                    else if (astNonterminal._signPost.MeaningUnit == this.ArrayVar)
                    {
                        var nodeCheckResult = this.CheckArrayVarNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                        result.DclItemData = nodeCheckResult.Data as DclItemData;
                    }
                }
            }

            return new NodeCheckResult(result, baseSymbolTable);
        }

        private object ActionCompoundSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            string result = string.Empty;

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal) continue;
                else
                {
                    var astNonterminal = item as TreeNonTerminal;
                    if (astNonterminal._signPost.MeaningUnit == this.DclList)
                        result += this.ActionDclList(astNonterminal, blockLevel, offset);
                    else if (astNonterminal._signPost.MeaningUnit == this.StatList)
                        result += this.ActionStatList(astNonterminal, blockLevel, offset);
                }
            }

            return result;
        }

        private NodeCheckResult CheckCompoundStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            var newSymbolTable = new MiniCSymbolTable(baseSymbolTable);

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal) continue;
                else
                {
                    var astNonterminal = item as TreeNonTerminal;
                    if (astNonterminal._signPost.MeaningUnit == this.DclList)
                    {
                        var nodeCheckResult = this.CheckDclListNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                        newSymbolTable = nodeCheckResult.symbolTable as MiniCSymbolTable;
                    }
                    else if (astNonterminal._signPost.MeaningUnit == this.StatList)
                    {
                        var nodeCheckResult = this.CheckStatListNode(astNonterminal, newSymbolTable, blockLevel, offset);
                    }
                }
            }

            node.ConnectedSymbolTable = newSymbolTable;

            return new NodeCheckResult(null, newSymbolTable);
        }

        private object ActionDclList(TreeNonTerminal node, int blockLevel, int offset)
        {
            string result = string.Empty;

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal) continue;
                else
                {
                    var astNonterminal = item as TreeNonTerminal;
                    if (astNonterminal._signPost.MeaningUnit == this.Dcl)
                        result += this.ActionDcl(astNonterminal, blockLevel, offset);
                }
            }

            return result;
        }

        private NodeCheckResult CheckDclListNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            MiniCSymbolTable newSymbolTable = new MiniCSymbolTable(baseSymbolTable);

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal) continue;
                else
                {
                    var astNonterminal = item as TreeNonTerminal;
                    if (astNonterminal._signPost.MeaningUnit == this.Dcl)
                    {
                        var nodeCheckResult = this.CheckDclNode(astNonterminal, baseSymbolTable, blockLevel, offset);

                        // add symbol information to the symbol table.
                        VarData varData = new VarData
                        {
                            DclData = nodeCheckResult.Data as DclData,
                            Offset = offset
                        };
                        newSymbolTable.VarDataList.Add(varData);
                        node.ConnectedSymbolTable = newSymbolTable; // connect the symbol table to the current node.
                    }
                }
            }

            return new NodeCheckResult(null, newSymbolTable);
        }

        private object ActionDcl(TreeNonTerminal node, int blockLevel, int offset)
        {
            DclData result = new DclData();
            result.BlockLevel = blockLevel;

            // const? (void | int | char) identifier ([integer])?
            foreach(var item in node.Items)
            {
                if (item is TreeTerminal) continue; // skip ; token

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.DclSpec)
                    result.DclSpecData = this.ActionDclSpec(astNonTerminal, blockLevel, offset) as DclSpecData;    // const? (void | int | char)
                else if (astNonTerminal._signPost.MeaningUnit == this.DclItem)
                    result.DclItemData = this.ActionDclItem(astNonTerminal, blockLevel, offset) as DclItemData;    // identifier ([integer])?
            }
 
            return UCode.DclVar(result.BlockLevel, offset, 1, result.DclItemData.Name);
        }

        private NodeCheckResult CheckDclNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclData result = new DclData();
            result.BlockLevel = blockLevel;

            // const? (void | int | char) identifier ([integer])?
            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue; // skip ; token

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.DclSpec)
                {
                    var nodeCheckResult = this.CheckDclSpecNode(astNonTerminal, baseSymbolTable, blockLevel, offset);    // const? (void | int | char)
                    result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                }
                else if (astNonTerminal._signPost.MeaningUnit == this.DclItem)
                {
                    var nodeCheckResult = this.CheckDclItemNode(astNonTerminal, baseSymbolTable, blockLevel, offset);    // identifier ([integer])?
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
                }
            }

            return new NodeCheckResult(result, baseSymbolTable);
        }

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

        private NodeCheckResult CheckDclItemNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            NodeCheckResult result = null;

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.SimpleVar)
                    result = this.CheckSimpleVarNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.ArrayVar)
                    result = this.CheckArrayVarNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }

            return new NodeCheckResult(result, baseSymbolTable);
        }

        private object ActionSimpleVar(TreeNonTerminal node, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData();

            result.NameToken = (node.Items[0] as TreeTerminal).Token;

            return result;
        }

        private NodeCheckResult CheckSimpleVarNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData();

            result.NameToken = (node.Items[0] as TreeTerminal).Token;

            return new NodeCheckResult(result, null);
        }

        private object ActionArrayVar(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private NodeCheckResult CheckArrayVarNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionStatList(TreeNonTerminal node, int blockLevel, int offset)
        {
            string result = string.Empty;

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.IfSt)
                    result += this.ActionIfSt(astNonTerminal, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.IfElseSt)
                    result += this.ActionIfElseSt(astNonTerminal, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.WhileSt)
                    result += this.ActionWhileSt(astNonTerminal, blockLevel, offset);
            }

            return result;
        }

        private NodeCheckResult CheckStatListNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            string result = string.Empty;

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.IfSt)
                    result += this.CheckIfStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.IfElseSt)
                    result += this.CheckIfElseStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.WhileSt)
                    result += this.CheckWhileStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }

            return new NodeCheckResult(null, baseSymbolTable);
        }

        private object ActionExpSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionIfSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private NodeCheckResult CheckIfStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionIfElseSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private NodeCheckResult CheckIfElseStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionWhileSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private NodeCheckResult CheckWhileStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
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

        private object ActionAdd(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionSub(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionMul(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionDiv(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionMod(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionAssign(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionAddAssign(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionSubAssign(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionMulAssign(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionDivAssign(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionModAssign(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

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
                this.CheckProgramNode(symbol as TreeNonTerminal, new MiniCSymbolTable(), 0, 0);
                return new SementicAnalysisResult(errList, null);
            }
            else return null;
        }

        public override string GenerateCode(TreeSymbol symbol)
        {
            return (symbol != null) ? this.ActionProgram(symbol as TreeNonTerminal, 0, 0) as string : string.Empty;
        }




        public MiniCSdts(KeyManager keyManager, MiniCGrammar grammar) : base(keyManager)
        {
            this.grammar = grammar;

            this.Program.ActionLogic = this.ActionProgram;
            this.FuncDef.ActionLogic = this.ActionFuncDef;
            this.FuncHead.ActionLogic = this.ActionFuncHead;
            this.DclSpec.ActionLogic = this.ActionDclSpec;
            this.FormalPara.ActionLogic = this.ActionFormalPara;
            this.ParamDcl.ActionLogic = this.ActionParamDcl;
            this.CompoundSt.ActionLogic = this.ActionCompoundSt;
            this.DclList.ActionLogic = this.ActionDclList;
            this.Dcl.ActionLogic = this.ActionDcl;
            this.DclItem.ActionLogic = this.ActionDclItem;
            this.SimpleVar.ActionLogic = this.ActionSimpleVar;
            this.ArrayVar.ActionLogic = this.ActionArrayVar;
            this.StatList.ActionLogic = this.ActionStatList;
            this.ExpSt.ActionLogic = this.ActionExpSt;
            this.IfSt.ActionLogic = this.ActionIfSt;
            this.IfElseSt.ActionLogic = this.ActionIfElseSt;
            this.WhileSt.ActionLogic = this.ActionWhileSt;
            this.ReturnSt.ActionLogic = this.ActionReturnSt;
            this.Index.ActionLogic = this.ActionIndex;
            this.Cell.ActionLogic = this.ActionCell;
            this.ActualParam.ActionLogic = this.ActionActualParam;

            this.Add.ActionLogic = this.ActionAdd;
            this.Sub.ActionLogic = this.ActionSub;
            this.Mul.ActionLogic = this.ActionMul;
            this.Div.ActionLogic = this.ActionDiv;
            this.Mod.ActionLogic = this.ActionMod;
            this.Assign.ActionLogic = this.ActionAssign;
            this.AddAssign.ActionLogic = this.ActionAddAssign;
            this.SubAssign.ActionLogic = this.ActionSubAssign;
            this.MulAssign.ActionLogic = this.ActionMulAssign;
            this.DivAssign.ActionLogic = this.ActionDivAssign;
            this.ModAssign.ActionLogic = this.ActionModAssign;
            this.LogicalOr.ActionLogic = this.ActionLogicalOr;
            this.LogicalAnd.ActionLogic = this.ActionLogicalAnd;
            this.LogicalNot.ActionLogic = this.ActionLogicalNot;
            this.Equal.ActionLogic = this.ActionEqual;
            this.NotEqual.ActionLogic = this.ActionNotEqual;
            this.GreaterThan.ActionLogic = this.ActionGreaterThan;
            this.LessThan.ActionLogic = this.ActionLessThan;
            this.GreatherEqual.ActionLogic = this.ActionGreatherEqual;
            this.LessEqual.ActionLogic = this.ActionLessEqual;
            this.UnaryMinus.ActionLogic = this.ActionUnaryMinus;
            this.PreInc.ActionLogic = this.ActionPreInc;
            this.PreDec.ActionLogic = this.ActionPreDec;
            this.PostInc.ActionLogic = this.ActionPostInc;
            this.PostDec.ActionLogic = this.ActionPostDec;

        }
    }
}
