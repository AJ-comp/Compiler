using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
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
                    var nodeCheckResult = this.BuildDclNode(astNonTerminal, baseSymbolTable, blockLevel, offset++);
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
            curNode.ConnectedInterLanguage.Add(UCode.Command.ProcStart(result.Name, 0, blockLevel, result.Name + " function"));

            return new NodeBuildResult(result, null);
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
                    var nodeCheckResult = this.BuildParamDcl(astNonterminal, baseSymbolTable, blockLevel, offset);
                    varData.DclData = nodeCheckResult.Data as DclData;
                    varData.Offset = offset++;
                    result.Add(varData);
                }
            }

            // params
            foreach (var item in result)
                curNode.ConnectedInterLanguage.Add(UCode.Command.DclVar(item.DclData.BlockLevel, item.Offset,
                                                                                                            item.DclData.DclItemData.Dimension,
                                                                                                            item.DclData.DclItemData.Name));

            return new NodeBuildResult(result, baseSymbolTable);
        }

        // format summary
        // DclSpec (SimpleVar | ArrayVar)
        private NodeBuildResult BuildParamDcl(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
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

            curNode.ConnectedInterLanguage.Add(UCode.Command.DclVar(result.BlockLevel, offset, result.DclItemData.Dimension, result.DclItemData.Name));

            return new NodeBuildResult(result, baseSymbolTable);
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
                    var nodeCheckResult = this.BuildDclNode(astNonterminal, baseSymbolTable, blockLevel, offset);

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

        // format summary
        // const? (void | int | char) identifier ([integer])? ;
        private NodeBuildResult BuildDclNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclData result = new DclData
            {
                BlockLevel = blockLevel
            };
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
                    var nodeCheckResult = this.BuildDclItemNode(astNonTerminal, baseSymbolTable, blockLevel, offset);    // identifier ([integer])?
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
                }
            }

            node.ConnectedInterLanguage.Add(UCode.Command.DclVar(result.BlockLevel, offset, result.DclItemData.Dimension, result.DclItemData.Name));

            return new NodeBuildResult(result, baseSymbolTable);
        }

        private NodeBuildResult BuildDclItemNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
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

        private NodeBuildResult BuildSimpleVarNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (node.Items[0] as TreeTerminal).Token
            };

            return new NodeBuildResult(result, null);
        }

        private NodeBuildResult BuildArrayVarNode(TreeNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }
    }
}
