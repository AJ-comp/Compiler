using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.InterLanguages;
using System.Collections.Generic;

using IR = Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        // [0:n] : Dcl? (AstNonTerminal)
        // [n+1:y] : FuncDef? (AstNonTerminal)
        private AstBuildResult BuildProgramNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            MiniCAstBuildParams buildParams = p as MiniCAstBuildParams;

            int funcOffset = 0;
            _labels.Clear();
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = buildParams.SymbolTable;

            foreach (var item in curNode.Items)
            {
                var astNonTerminal = item as AstNonTerminal;

                // Global variable
                if (astNonTerminal.SignPost.MeaningUnit == this.Dcl)
                {
                    var dclBuildResult = astNonTerminal.BuildLogic(buildParams, astNodes);

                    // set data to symbol table
                    foreach(var dclData in dclBuildResult.Data as List<DclData>)
                    {
                        RealVarData varData = new RealVarData(dclData);
                        (buildParams.SymbolTable as MiniCSymbolTable).VarDataList.Add(varData);
                    }
                }
                // Global function
                else if (astNonTerminal.SignPost.MeaningUnit == this.FuncDef)
                {
                    buildParams.Offset = 0;
                    var funcDefBuildResult = astNonTerminal.BuildLogic(buildParams, astNodes);

                    // set data to symbol table
                    var funcData = funcDefBuildResult.Data as FuncData;
                    funcData.Offset = funcOffset++;
                    (buildParams.SymbolTable as MiniCSymbolTable).FuncDataList.Add(funcData);
                }
            }

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(null, null, true);
        }

        // [0] : FuncHead (AstNonTerminal)
        // [1] : CompoundSt (AstNonTerminal)
        private AstBuildResult BuildFuncDefNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            var newSymbolTable = new MiniCSymbolTable(p.SymbolTable as MiniCSymbolTable);
            curNode.ClearConnectedInfo();

            // ready to build
            var buildParams = p.Clone() as MiniCAstBuildParams;
            buildParams.SymbolTable = newSymbolTable;
            buildParams.BlockLevel++;

            // build FuncHead node
            var node0 = curNode[0] as AstNonTerminal;
            var funcHeadResult = node0.BuildLogic(buildParams, astNodes);
            var funcHeadData = funcHeadResult.Data as FuncData;

            // set datas
            funcHeadData.This = true;
            _labels.Add(funcHeadData.Name);
            newSymbolTable.FuncDataList.Add(funcHeadData);
            newSymbolTable.VarDataList.AddRange(funcHeadData.ParamVars);
            curNode.ConnectedSymbolTable = newSymbolTable;

            // build CompoundSt node
            var node1 = curNode[1] as AstNonTerminal;
            var compStResult = node1.BuildLogic(buildParams, astNodes);

            IROptions options = new IROptions(ReservedLabel);
            IR.FuncData funcData = IRConverter.ToIRData(funcHeadData);
            curNode.ConnectedIrUnits.Add(IRBuilder.CreateDefineFunction(options, funcData, );
            astNodes.Add(curNode);

            return new AstBuildResult(funcHeadData, newSymbolTable, true);
        }

        // [0] : DclSpec (AstNonTerminal)
        // [1] : Name (AstTerminal)
        // [2] : FormalPara (AstNonTerminal)
        private AstBuildResult BuildFuncHeadNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            FuncData result = new FuncData();
            curNode.ClearConnectedInfo();

            // ready to build
            var buildParams = p as MiniCAstBuildParams;

            // build DclSpec node
            var node0 = curNode[0] as AstNonTerminal;
            var node0Result = node0.BuildLogic(p, astNodes);
            result.DclSpecData = node0Result.Data as DclSpecData;

            // set Name data
            var token = curNode[1] as AstTerminal;
            result.NameToken = token.Token;

            // add function start IR
            //            curNode.ConnectedIrUnits.Add(IRBuilder.(result.Name, 0, buildParams.BlockLevel, result.Name + " function"));
            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            // build FormalPara node
            var node2 = curNode[2] as AstNonTerminal;
            var node2Result = node2.BuildLogic(p, astNodes);
            result.ParamVars.AddRange(node2Result.Data as List<RealVarData>);

            return new AstBuildResult(result, null, true);
        }

        private AstBuildResult BuildDclSpecNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
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
                    result.DataType = SymbolTableFormat.DataType.Void;
                    result.DataTypeToken = (astNonTerminal.Items[0] as AstTerminal).Token;
                }
                else if (astNonTerminal.SignPost.MeaningUnit == this.IntNode)
                {
                    result.DataType = SymbolTableFormat.DataType.Int;
                    result.DataTypeToken = (astNonTerminal.Items[0] as AstTerminal).Token;
                }
            }

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(result, null, true);
        }

        // format summary [Can induce epsilon]
        // [0:n] : ParamDcl (AstNonTerminal)
        private AstBuildResult BuildFormalParaNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            List<RealVarData> result = new List<RealVarData>();
            curNode.ClearConnectedInfo();

            // ready to build
            var buildParams = p as MiniCAstBuildParams;

            foreach (var item in curNode.Items)
            {
                var paramDclNode = item as AstNonTerminal;
                var paramDclResult = paramDclNode.BuildLogic(buildParams, astNodes);
                buildParams.Offset++;

                result.Add(new RealVarData(paramDclResult.Data as DclData));
            }

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(result, buildParams.SymbolTable, true);
        }

        // format summary
        // [0] : DclSpec (AstNonTerminal)
        // [1] : (SimpleVar | ArrayVar) (AstNonTerminal)
        private AstBuildResult BuildParamDcl(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            // ready to build
            var buildParams = p as MiniCAstBuildParams;

            DclData result = new DclData
            {
                BlockLevel = buildParams.BlockLevel,
                Offset = buildParams.Offset,
                Etc = EtcInfo.Param
            };

            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                var nodeCheckResult = astNonterminal.BuildLogic(buildParams, astNodes);

                if (astNonterminal.SignPost.MeaningUnit == this.DclSpec)
                    result.DclSpecData = nodeCheckResult.Data as DclSpecData;
                else if (astNonterminal.SignPost.MeaningUnit == this.SimpleVar)
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
                else if (astNonterminal.SignPost.MeaningUnit == this.ArrayVar)
                    result.DclItemData = nodeCheckResult.Data as DclItemData;
            }

            ConnectDclVarIRToNode(curNode, buildParams, result);

            astNodes.Add(curNode);

            return new AstBuildResult(result, null, true);
        }

        // format summary [Can induce epsilon]
        // [0:n] : Dcl (AstNonTerminal)
        private AstBuildResult BuildDclListNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            MiniCSymbolTable newSymbolTable = new MiniCSymbolTable(p.SymbolTable as MiniCSymbolTable);
            curNode.ConnectedSymbolTable = newSymbolTable; // connect the symbol table to the current node.

            foreach (var item in curNode.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var dclNode = item as AstNonTerminal;
                var dclBuildResult = dclNode.BuildLogic(p, astNodes);
            }

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(null, newSymbolTable, true);
        }

        // format summary
        // [0] : DclSpec (AstNonTerminal)
        // [1] : InitDeclarator (AstNonTerminal)
        private AstBuildResult BuildDclNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            List<DclData> result = new List<DclData>();

            // build DclSpec node
            var specNT = curNode[0] as AstNonTerminal;
            var buildResult = specNT.BuildLogic(p, astNodes);
            var specData = buildResult.Data as DclSpecData;

            // ready to build InitDeclarator node
            var buildParams = p as MiniCAstBuildParams;

            // build InitDeclarator nodes
            for (int i = 1; i < curNode.Items.Count; i++)
            {
                var declNT = curNode[i] as AstNonTerminal;
                var declBuildResult = declNT.BuildLogic(buildParams, astNodes);
                var dclItemData = declBuildResult.Data as DclItemData;

                var dclData = new DclData()
                {
                    BlockLevel = buildParams.BlockLevel,
                    Offset = buildParams.Offset,
                    DclSpecData = specData,
                    DclItemData = dclItemData
                };
                result.Add(dclData);

                ConnectDclVarIRToNode(curNode, buildParams, dclData);
            }

            astNodes.Add(curNode);

            return new AstBuildResult(result, null, true);
        }

        // format summary
        // [0] : (SimpleVar | ArrayVar) (AstNonTerminal)
        // [1] : LiteralNode? (AstNonTerminal)
        private AstBuildResult BuildDclItemNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            var astNonTerminal = curNode[0] as AstNonTerminal;
            var data = astNonTerminal.BuildLogic(p, astNodes).Data as DclItemData;

            if(curNode.Count > 1)
            {
                astNonTerminal = curNode[1] as AstNonTerminal;
                var literalData = astNonTerminal.BuildLogic(p, astNodes).Data as LiteralData;
                data.Value = literalData;
            }

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(data, null, true);
        }

        // format summary
        // [0] : Ident (AstTerminal)
        private AstBuildResult BuildSimpleVarNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (curNode.Items[0] as AstTerminal).Token
            };

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(result, null, true);
        }

        // format summary
        // [0] : ident (AstTerminal)
        // [1] : number (AstTerminal)
        private AstBuildResult BuildArrayVarNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            DclItemData result = new DclItemData
            {
                NameToken = (curNode.Items[0] as AstTerminal).Token,
                DimensionToken = (curNode.Items[1] as AstTerminal).Token
            };

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(result, null, true);
        }

        private AstBuildResult BuildVarNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            bool result = true;
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = p.SymbolTable;

            var varData = ConnectSimpleVarCode(curNode.Items[0] as AstTerminal, p);
            if (varData is VirtualVarData) result = false;

            // it has to add 'curNode.Items[0]' to the AstNodes because IL or ME is added in the ConnectSimpleVarCode function.            
            astNodes.Add(curNode.Items[0]);

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(varData, null, result);
        }

        private AstBuildResult BuildIntLiteralNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = p.SymbolTable;

            IntLiteralData result = new IntLiteralData((curNode.Items[0] as AstTerminal).Token);
            curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.DclValue(ReservedLabel, result.Value));
            astNodes.Add(curNode);

            return new AstBuildResult(result, null, true);
        }
    }
}
