using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;
using System;
using System.Collections.Generic;
using AlarmCodes = Parse.FrontEnd.Grammars.Properties.AlarmCodes;

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

        private object BuildProgramNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            int funcOffset = 0;
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return null;

            foreach (var item in curNode.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                // Global variable
                if (astNonTerminal._signPost.MeaningUnit == this.Dcl)
                {
                    var nodeCheckResult = this.CheckDclNode(astNonTerminal, baseSymbolTable, blockLevel, offset++);
                    VarData varData = new VarData
                    {
                        DclData = nodeCheckResult.Data as DclData,
                        Offset = offset
                    };
                    baseSymbolTable.VarDataList.Add(varData);
                    curNode.ConnectedSymbolTable = baseSymbolTable;
                }
                // Global function
                else if (astNonTerminal._signPost.MeaningUnit == this.FuncDef)
                {
                    var nodeCheckResult = this.BuildFuncDefNode(astNonTerminal, baseSymbolTable, blockLevel, 0);
                    var funcData = nodeCheckResult.Data as FuncData;
                    funcData.Offset = funcOffset++;
                    baseSymbolTable.FuncDataList.Add(funcData);
                }
            }

            return null;
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

        private NodeBuildResult BuildFuncDefNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            FuncData funcHeadData = new FuncData();
            var newSymbolTable = new MiniCSymbolTable(baseSymbolTable);
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(funcHeadData, newSymbolTable);

            foreach (var item in curNode.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.FuncHead)
                {
                    var nodeCheckResult = this.BuildFuncHeadNode(item as TreeNonTerminal, newSymbolTable, blockLevel + 1, offset);
                    funcHeadData = nodeCheckResult.Data as FuncData;
                    offset = funcHeadData.ParamVars.Count;

                    funcHeadData.This = true;
                    newSymbolTable.FuncDataList.Add(funcHeadData);
                    newSymbolTable.VarDataList.AddRange(funcHeadData.ParamVars);

                    curNode.ConnectedSymbolTable = newSymbolTable;
                }
                else if (astNonTerminal._signPost.MeaningUnit == this.CompoundSt)
                    this.BuildCompoundStNode(item as TreeNonTerminal, newSymbolTable, blockLevel + 1, offset);
            }

            return new NodeBuildResult(funcHeadData, newSymbolTable);
        }

        private object ActionFuncHead(TreeNonTerminal curNode, int blockLevel, int offset)
        {
            List<TreeNonTerminal> result = new List<TreeNonTerminal>();

            result.Add(curNode);

            foreach(var item in curNode.Items)
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

        // format summary
        // DclSpec Ident FormalPara
        private NodeBuildResult BuildFuncHeadNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            FuncData result = new FuncData();
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(result, null);

            foreach (var item in curNode.Items)
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
                    if (astNonterminal._signPost.MeaningUnit == this.DclSpec)   // return type
                    {
                        var nodeCheckResult = this.BuildDclSpecNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                        result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                    }
                    else if (astNonterminal._signPost.MeaningUnit == this.FormalPara)
                    {
                        var nodeCheckResult = this.BuildFormalParaNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                        var datas = nodeCheckResult.Data as List<VarData>;
                        result.ParamVars.AddRange(datas);
                    }
                }
            }

            // function start
            curNode.ConnectedInterLanguage = UCode.ProcStart(result.Name, 0, blockLevel, result.Name + " function");

            return new NodeBuildResult(result, null);
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

        private NodeBuildResult BuildDclSpecNode(TreeNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclSpecData result = new DclSpecData();
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(result, baseSymbolTable);

            foreach (var item in curNode.Items)
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

            return new NodeBuildResult(result, baseSymbolTable);
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

        // format summary
        // ( ParamDcl? )
        private NodeBuildResult BuildFormalParaNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            List<VarData> result = new List<VarData>();
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(result, baseSymbolTable);

            foreach (var item in curNode.Items)
            {
                if (item is TreeTerminal) continue;   // skip ( token and ) token

                VarData varData = new VarData();
                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.ParamDcl)
                {
                    var nodeCheckResult = this.CheckParamDcl(astNonterminal, baseSymbolTable, blockLevel, offset);
                    varData.DclData = nodeCheckResult.Data as DclData;
                    varData.Offset = offset++;
                    result.Add(varData);
                }
            }

            // params
            foreach (var item in result)
                curNode.ConnectedInterLanguage = UCode.DclVar(item.DclData.BlockLevel, item.Offset, item.DclData.DclItemData.Dimension, item.DclData.DclItemData.Name);

            return new NodeBuildResult(result, baseSymbolTable);
        }

        private object ActionParamDcl(TreeNonTerminal curNode, int blockLevel, int offset)
        {
            return curNode;
        }

        // format summary
        // DclSpec (SimpleVar | ArrayVar)
        private NodeBuildResult CheckParamDcl(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclData result = new DclData
            {
                BlockLevel = blockLevel,
                Etc = EtcInfo.Param
            };

            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(result, baseSymbolTable);

            foreach (var item in curNode.Items)
            {
                // ident
                if (item is TreeTerminal) continue;

                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.DclSpec)
                {
                    var nodeCheckResult = this.BuildDclSpecNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                    result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                }
                else if (astNonterminal._signPost.MeaningUnit == this.SimpleVar)
                {
                    var nodeCheckResult = this.BuildSimpleVarNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
                }
                else if (astNonterminal._signPost.MeaningUnit == this.ArrayVar)
                {
                    var nodeCheckResult = this.BuildArrayVarNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
                }
            }

            curNode.ConnectedInterLanguage = UCode.DclVar(result.BlockLevel, offset, result.DclItemData.Dimension);

            return new NodeBuildResult(result, baseSymbolTable);
        }

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

        private NodeBuildResult BuildCompoundStNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            var newSymbolTable = new MiniCSymbolTable(baseSymbolTable);
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(null, newSymbolTable);

            foreach (var item in curNode.Items)
            {
                // ident
                if (item is TreeTerminal) continue;

                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.DclList)
                {
                    var nodeCheckResult = this.BuildDclListNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                    newSymbolTable = nodeCheckResult.symbolTable as MiniCSymbolTable;
                }
                else if (astNonterminal._signPost.MeaningUnit == this.StatList)
                {
                    var nodeCheckResult = this.BuildStatListNode(astNonterminal, newSymbolTable, blockLevel, offset);
                }
            }

            curNode.ConnectedSymbolTable = newSymbolTable;

            return new NodeBuildResult(null, newSymbolTable);
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

        private NodeBuildResult BuildDclListNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            MiniCSymbolTable newSymbolTable = new MiniCSymbolTable(baseSymbolTable);

            foreach (var item in node.Items)
            {
                // ident
                if (item is TreeTerminal) continue;

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

            return new NodeBuildResult(null, newSymbolTable);
        }

        private object ActionDcl(TreeNonTerminal node, int blockLevel, int offset)
        {
            return node;
        }

        // format summary
        // const? (void | int | char) identifier ([integer])? ;
        private NodeBuildResult CheckDclNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclData result = new DclData();
            result.BlockLevel = blockLevel;
            if (node.HasVirtualChild) return new NodeBuildResult(result, baseSymbolTable);

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue; // skip ; token

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.DclSpec)
                {
                    var nodeCheckResult = this.BuildDclSpecNode(astNonTerminal, baseSymbolTable, blockLevel, offset);    // const? (void | int | char)
                    result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                }
                else if (astNonTerminal._signPost.MeaningUnit == this.DclItem)
                {
                    var nodeCheckResult = this.CheckDclItemNode(astNonTerminal, baseSymbolTable, blockLevel, offset);    // identifier ([integer])?
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
                }
            }

            node.ConnectedInterLanguage = UCode.DclVar(result.BlockLevel, offset, result.DclItemData.Dimension, result.DclItemData.Name);

            return new NodeBuildResult(result, baseSymbolTable);
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

        private NodeBuildResult CheckDclItemNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            NodeBuildResult result = null;

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.SimpleVar)
                    result = this.BuildSimpleVarNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.ArrayVar)
                    result = this.BuildArrayVarNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }

            return result;
        }

        private object ActionSimpleVar(TreeNonTerminal node, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData();

            result.NameToken = (node.Items[0] as TreeTerminal).Token;

            return result;
        }

        private NodeBuildResult BuildSimpleVarNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData();

            result.NameToken = (node.Items[0] as TreeTerminal).Token;

            return new NodeBuildResult(result, null);
        }

        private object ActionArrayVar(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildArrayVarNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
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

        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt
        private NodeBuildResult BuildStatListNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            string result = string.Empty;
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(null, baseSymbolTable);

            foreach (var item in curNode.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.IfSt)
                    result += this.BuildIfStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.IfElseSt)
                    result += this.BuildIfElseStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.WhileSt)
                    result += this.BuildWhileStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.ExpSt)
                    result += this.BuildExpStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }

            return new NodeBuildResult(null, baseSymbolTable);
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

        // format summary
        // (AddAssign | SubAssign | ...) ;
        private NodeBuildResult BuildExpStNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            string result = string.Empty;
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(null, baseSymbolTable);

            foreach (var item in curNode.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.AddAssign)
                    result += this.BuildAddAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }

            return new NodeBuildResult(null, baseSymbolTable);
        }

        private object ActionIfSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildIfStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionIfElseSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildIfElseStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionWhileSt(TreeNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildWhileStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
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

        private object ActionAddAssign(TreeNonTerminal curNode, int blockLevel, int offset)
        {
            return curNode;
        }

        /// <summary>
        /// This function checks whether exist variable in the 'baseSymbolTable'.
        /// </summary>
        /// <param name="childNodeToCheck">The child node to check</param>
        /// <param name="baseSymbolTable">The symbol table to reference</param>
        /// <returns></returns>
        private VarData CheckExistVarInSymbolTable(TreeTerminal childNodeToCheck, MiniCSymbolTable baseSymbolTable)
        {
            VarData result = null;

            if(childNodeToCheck.Token.Kind.TokenType is Identifier)
                result = baseSymbolTable.AllVarList.GetVarByName(childNodeToCheck.Token.Input);

            return result;
        }

        private VarData BuildSimpleAssignForVar(TreeNonTerminal node, int varIndex)
        {
            TreeTerminal varNode = node[varIndex] as TreeTerminal;
            var varData = CheckExistVarInSymbolTable(varNode, node.ConnectedSymbolTable as MiniCSymbolTable);
            if (varData == null)
            {
                node.ConnectedErrInfoList.Add(new MeaningErrInfo(varNode.Token,
                                                                                            string.Format(AlarmCodes.MCL0001, varNode.Token.Input)));
            }
            else if (varData.DclData.DclSpecData.Const)
                node.ConnectedErrInfoList.Add(new MeaningErrInfo(varNode.Token, AlarmCodes.MCL0002));
            else
                node.ConnectedInterLanguage += UCode.LoadVar(varData.DclData.BlockLevel, varData.Offset, varData.DclData.DclItemData.Name);

            return varData;
        }

        private bool BuildSimpleAssignForValue(TreeNonTerminal node, int valueIndex)
        {
            bool result = true;

            // if valueTerminal is Ident
            TreeTerminal valueNode = node[valueIndex] as TreeTerminal;
            if (valueNode.Token.Kind.TokenType is Identifier)
            {
                var valueData = CheckExistVarInSymbolTable(valueNode, node.ConnectedSymbolTable as MiniCSymbolTable);
                if (valueData != null)
                    node.ConnectedInterLanguage += UCode.LoadVar(valueData.DclData.BlockLevel, valueData.Offset, valueData.DclData.DclItemData.Name);
                else
                {
                    result = false;
                    node.ConnectedErrInfoList.Add(new MeaningErrInfo(valueNode.Token,
                                                                                                string.Format(AlarmCodes.MCL0001, valueNode.Token.Input)));
                }
            }
            // if valueTerminal is digit
            else if (valueNode.Token.Kind.TokenType is Digit)
            {

            }

            return result;
        }

        /// <summary>
        /// This function adds tree information for the child node of the 'node' to the 'node'.
        /// </summary>
        /// <param name="node">The node to add information for a child node</param>
        /// <param name="varIndex">The child node index to check</param>
        /// <param name="valueIndex">The child node index to check</param>
        /// <returns>If error information was added return null, else returns VarData of the varIndex.</returns>
        private VarData BuildSimpleAssignReady(TreeNonTerminal node, int varIndex, int valueIndex)
        {
            // varTerminal is Ident.
            // valueTerminal is (Ident | digit)

            // varTerminal is Ident
            var varData = BuildSimpleAssignForVar(node, varIndex);

            // if valueTerminal is Ident
            TreeTerminal valueNode = node[valueIndex] as TreeTerminal;
            if (valueNode.Token.Kind.TokenType is Identifier)
            {
                var valueData = CheckExistVarInSymbolTable(valueNode, node.ConnectedSymbolTable as MiniCSymbolTable);
                if (valueData == null)
                {
                    varData = null;
                    node.ConnectedErrInfoList.Add(new MeaningErrInfo(valueNode.Token,
                                                                                                string.Format(AlarmCodes.MCL0001, valueNode.Token.Input)));
                }
                else if(valueData.DclData.DclSpecData.DataType != varData?.DclData.DclSpecData.DataType)
                {
                    varData = null;
                    node.ConnectedErrInfoList.Add(new MeaningErrInfo(valueNode.Token, AlarmCodes.MCL0003));
                }
                else
                    node.ConnectedInterLanguage += UCode.LoadVar(valueData.DclData.BlockLevel, valueData.Offset, valueData.DclData.DclItemData.Name);
            }
            // if valueTerminal is digit
            else if (valueNode.Token.Kind.TokenType is Digit)
            {
                node.ConnectedInterLanguage += UCode.DclValue(System.Convert.ToInt32(valueNode.Token.Input));
            }

            return varData;
        }

        // format summary
        // ident += (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildAddAssignNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(null, baseSymbolTable);

            curNode.ConnectedSymbolTable = baseSymbolTable;

            // [0] is ident
            // [2] is (ident | digit)
            var result = BuildSimpleAssignReady(curNode, 0, 2);

            if (result != null)
            {
                curNode.ConnectedInterLanguage += UCode.Add();
                curNode.ConnectedInterLanguage += UCode.Store(result.DclData.BlockLevel, result.Offset, result.DclData.DclItemData.Name);
            }
            else curNode.ConnectedInterLanguage = string.Empty;

            return new NodeBuildResult(null, baseSymbolTable);
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
                this.BuildProgramNode(symbol as TreeNonTerminal, new MiniCSymbolTable(), 0, 0);
                return new SementicAnalysisResult(errList, null);
            }
            else return null;
        }

        public override string GenerateCode(TreeSymbol symbol)
        {
            string result = string.Empty;

            if(symbol != null)
            {
                var trees = this.ActionProgram(symbol as TreeNonTerminal, 0, 0) as List<TreeNonTerminal>;

                foreach(var item in trees)
                {
                    result += item.ConnectedInterLanguage;
                }
            }

            return result;
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
