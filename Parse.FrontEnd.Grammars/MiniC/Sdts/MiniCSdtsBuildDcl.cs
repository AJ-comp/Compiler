using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        // [0:n] : Dcl (AstNonTerminal)
        // [n+1:y] : FuncDef (AstNonTerminal)
        private AstBuildResult BuildProgramNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            int funcOffset = 0;
            _labels.Clear();
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;

            foreach (var item in curNode.Items)
            {
                var astNonTerminal = item as AstNonTerminal;

                // Global variable
                if (astNonTerminal.SignPost.MeaningUnit == this.Dcl)
                {
                    var nodeCheckResult = this.BuildDclNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                    VarData varData = new VarData
                    {
                        DclData = nodeCheckResult.Data as DclData,
                        Offset = offset
                    };
                    (baseSymbolTable as MiniCSymbolTable).VarDataList.Add(varData);
                    offset++;
                }
                // Global function
                else if (astNonTerminal.SignPost.MeaningUnit == this.FuncDef)
                {
                    var nodeCheckResult = this.BuildFuncDefNode(astNonTerminal, baseSymbolTable, blockLevel, 0);
                    var funcData = nodeCheckResult.Data as FuncData;
                    funcData.Offset = funcOffset++;
                    (baseSymbolTable as MiniCSymbolTable).FuncDataList.Add(funcData);
                }
            }

            return null;
        }

        private AstBuildResult BuildFuncDefNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            FuncData funcHeadData = new FuncData();
            var newSymbolTable = new MiniCSymbolTable(baseSymbolTable as MiniCSymbolTable);
            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
            {
                var astNonTerminal = item as AstNonTerminal;

                if (astNonTerminal.SignPost.MeaningUnit == this.FuncHead)
                {
                    var nodeCheckResult = this.BuildFuncHeadNode(astNonTerminal, newSymbolTable, blockLevel + 1, offset);
                    funcHeadData = nodeCheckResult.Data as FuncData;
                    offset = funcHeadData.ParamVars.Count;

                    funcHeadData.This = true;
                    _labels.Add(funcHeadData.Name);
                    newSymbolTable.FuncDataList.Add(funcHeadData);
                    newSymbolTable.VarDataList.AddRange(funcHeadData.ParamVars);

                    curNode.ConnectedSymbolTable = newSymbolTable;
                }
                else if (astNonTerminal.SignPost.MeaningUnit == this.CompoundSt)
                {
                    var nodeCheckResult = this.BuildCompoundStNode(astNonTerminal, newSymbolTable, blockLevel + 1, offset);
                }
            }

            return new AstBuildResult(funcHeadData, newSymbolTable);
        }

        // format summary
        // DclSpec Ident FormalPara
        private AstBuildResult BuildFuncHeadNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            FuncData result = new FuncData();
            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
            {
                // ident
                if (item is AstTerminal)
                {
                    var token = item as AstTerminal;

                    result.NameToken = token.Token;
                    continue;
                }

                var astNonterminal = item as AstNonTerminal;
                var nodeCheckResult = astNonterminal.BuildLogic(baseSymbolTable, blockLevel, offset);

                if (astNonterminal.SignPost.MeaningUnit == this.DclSpec)   // return type
                {
                    result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                }
                else if (astNonterminal.SignPost.MeaningUnit == this.FormalPara)
                {
                    var datas = nodeCheckResult.Data as List<VarData>;
                    result.ParamVars.AddRange(datas);
                }
            }

            // function start
            curNode.ConnectedInterLanguage.Add(UCode.Command.ProcStart(result.Name, 0, blockLevel, result.Name + " function"));

            return new AstBuildResult(result, null);
        }

        private AstBuildResult BuildDclSpecNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclSpecData result = new DclSpecData();
            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
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

            return new AstBuildResult(result, baseSymbolTable);
        }

        // format summary [Can induce epsilon]
        // [0:n] : ParamDcl (AstNonTerminal)
        private AstBuildResult BuildFormalParaNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            List<VarData> result = new List<VarData>();
            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;   // skip ( token and ) token

                VarData varData = new VarData();
                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.ParamDcl)
                {
                    var nodeCheckResult = astNonterminal.BuildLogic(baseSymbolTable, blockLevel, offset);
                    varData.DclData = nodeCheckResult.Data as DclData;
                    varData.Offset = offset++;
                    result.Add(varData);
                }
            }

            // params
            foreach (var item in result)
                curNode.ConnectedInterLanguage.Add(UCode.Command.DclVar(ReservedLabel, item.DclData.BlockLevel, item.Offset,
                                                                                                            item.DclData.DclItemData.Dimension, item.DclData.DclItemData.Name));

            return new AstBuildResult(result, baseSymbolTable);
        }

        // format summary
        // DclSpec (SimpleVar | ArrayVar)
        private AstBuildResult BuildParamDcl(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclData result = new DclData
            {
                BlockLevel = blockLevel,
                Etc = EtcInfo.Param
            };

            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                var nodeCheckResult = astNonterminal.BuildLogic(baseSymbolTable, blockLevel, offset);

                if (astNonterminal.SignPost.MeaningUnit == this.DclSpec)
                    result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                else if (astNonterminal.SignPost.MeaningUnit == this.SimpleVar)
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
                else if (astNonterminal.SignPost.MeaningUnit == this.ArrayVar)
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
            }

            curNode.ConnectedInterLanguage.Add(UCode.Command.DclVar(ReservedLabel, result.BlockLevel, offset, 
                                                                                                        result.DclItemData.Dimension, result.DclItemData.Name));

            return new AstBuildResult(result, baseSymbolTable);
        }

        // format summary [Can induce epsilon]
        // [0:n] : Dcl (AstNonTerminal)
        private AstBuildResult BuildDclListNode(AstNonTerminal node, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            MiniCSymbolTable newSymbolTable = new MiniCSymbolTable(baseSymbolTable as MiniCSymbolTable);

            foreach (var item in node.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.Dcl)
                {
                    var nodeCheckResult = astNonterminal.BuildLogic(baseSymbolTable, blockLevel, offset);

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

            return new AstBuildResult(null, newSymbolTable);
        }

        // format summary
        // const? (void | int | char) identifier ([integer])? ;
        private AstBuildResult BuildDclNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclData result = new DclData
            {
                BlockLevel = blockLevel
            };

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue; // skip ; token

                var astNonTerminal = item as AstNonTerminal;
                var nodeCheckResult = astNonTerminal.BuildLogic(baseSymbolTable, blockLevel, offset);

                if (astNonTerminal.SignPost.MeaningUnit == this.DclSpec)    // const? (void | int | char)
                    result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                else if (astNonTerminal.SignPost.MeaningUnit == this.DclItem)    // identifier ([integer])?
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
            }

            curNode.ConnectedInterLanguage.Add(UCode.Command.DclVar(ReservedLabel, result.BlockLevel, offset, 
                                                                                                    result.DclItemData.Dimension, result.DclItemData.Name));

            return new AstBuildResult(result, baseSymbolTable);
        }

        // format summary
        // [0] : (SimpleVar | ArrayVar) (AstNonTerminal)
        private AstBuildResult BuildDclItemNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            AstBuildResult result = null;

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                result = astNonTerminal.BuildLogic(baseSymbolTable, blockLevel, offset);
            }

            return result;
        }

        // format summary
        // [0] : Ident (AstTerminal)
        private AstBuildResult BuildSimpleVarNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (curNode.Items[0] as AstTerminal).Token
            };

            return new AstBuildResult(result, null);
        }

        // format summary
        // [0] : Ident (AstTerminal)
        // [1] : Number (AstTerminal)
        private AstBuildResult BuildArrayVarNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }
    }
}
