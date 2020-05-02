using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Grammars.Properties;
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


        private object ActionProgram(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            int offset = 0;

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.Dcl)
                {
                    var dclData = this.ActionDcl(astNonTerminal, table, blockLevel, errList) as DclData;
                    table.Add(dclData.KeyString, new VarData() { DclData = dclData, Offset = offset++ });
                }
                else if(astNonTerminal._signPost.MeaningUnit == this.FuncDef)
                {
                    this.ActionFuncDef(astNonTerminal, table, blockLevel, errList);
                }
            }

            return table;
        }

        private object ActionFuncDef(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            FuncData result = new FuncData();

            foreach (var item in node.Items)
            {
                var astNonTerminal = item as TreeNonTerminal;

                if (astNonTerminal._signPost.MeaningUnit == this.FuncHead)
                {
                    var funcData = this.ActionFuncHead(item as TreeNonTerminal, table, blockLevel + 1, errList) as FuncData;
                    if (table.ContainsKey(funcData.KeyString))
                    {
                        this.SementicErrorEventHandler?.Invoke(this,
                            new SementicErrorArgs(funcData.NameToken, AlarmCodes.MCL0000, string.Format(AlarmCodes.MCL0000, funcData.Name),
                            ErrorType.Error));
                    }
                    else table.Add(funcData.KeyString, funcData);
                }
                else if (astNonTerminal._signPost.MeaningUnit == this.CompoundSt)
                {
                    var varDatas = this.ActionCompoundSt(item as TreeNonTerminal, table, blockLevel + 1, errList) as List<VarData>;

                }
            }

            return result;
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
        private object ActionFuncHead(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
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
                        result.DclSpecData = this.ActionDclSpec(astNonterminal, table, blockLevel, errList) as DclSpecData;
                    else if (astNonterminal._signPost.MeaningUnit == this.FormalPara)
                    {
                        var varDatas = this.ActionFormalPara(astNonterminal, table, blockLevel, errList) as List<VarData>;
                        result.LocalVars.AddRange(varDatas);
                    }
                }
            }

            return result;
        }

        private object ActionDclSpec(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
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

        private object ActionFormalPara(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            List<VarData> result = new List<VarData>();

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;   // skip ( token and ) token

                VarData varData = new VarData();
                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.ParamDcl)
                {
                    varData.DclData = this.ActionParamDcl(astNonterminal, table, blockLevel, errList) as DclData;
                    result.Add(varData);
                }
            }

            return result;
        }

        private object ActionParamDcl(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
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
                        result.DclSpecData = this.ActionDclSpec(astNonterminal, table, blockLevel, errList) as DclSpecData;
                }
            }

            return result;
        }

        private object ActionCompoundSt(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return false;
        }

        private object ActionDclList(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionDcl(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            DclData result = new DclData();
            result.BlockLevel = blockLevel;

            foreach(var item in node.Items)
            {
                if (item is TreeTerminal) continue; // skip ; token

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.DclSpec)
                    result.DclSpecData = this.ActionDclSpec(astNonTerminal, table, blockLevel, errList) as DclSpecData;
                else if (astNonTerminal._signPost.MeaningUnit == this.DclItem)
                    result.DclItemData = this.ActionDclItem(astNonTerminal, table, blockLevel, errList) as DclItemData;
            }

            return result;
        }

        private object ActionDclItem(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            DclItemData result = new DclItemData();

            foreach (var item in node.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.SimpleVar)
                    result = this.ActionSimpleVar(astNonTerminal, table, blockLevel, errList) as DclItemData;
                else if (astNonTerminal._signPost.MeaningUnit == this.ArrayVar)
                    result = this.ActionArrayVar(astNonTerminal, table, blockLevel, errList) as DclItemData;
            }

            return result;
        }

        private object ActionSimpleVar(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            DclItemData result = new DclItemData();

            result.NameToken = (node.Items[0] as TreeTerminal).Token;

            return result;
        }

        private object ActionArrayVar(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionStatList(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionExpSt(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionIfSt(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionIfElseSt(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionWhileSt(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionReturnSt(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionIndex(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionCell(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionActualParam(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionAdd(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionSub(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionMul(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionDiv(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionMod(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionAssign(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionAddAssign(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionSubAssign(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionMulAssign(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionDivAssign(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionModAssign(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLogicalOr(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLogicalAnd(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLogicalNot(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionEqual(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionNotEqual(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionGreaterThan(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLessThan(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionGreatherEqual(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLessEqual(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionUnaryMinus(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionPreInc(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionPreDec(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionPostInc(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionPostDec(TreeNonTerminal node, SymbolTable table, int blockLevel, MeaningErrInfoList errList)
        {
            return null;
        }

        public override SementicAnalysisResult Process(TreeSymbol symbol)
        {
            MeaningErrInfoList errList = new MeaningErrInfoList();
            var table = new SymbolTable();

            if (symbol != null)
            {
                this.ActionProgram(symbol as TreeNonTerminal, table, 0, errList);
                return new SementicAnalysisResult(errList, table);
            }
            else return null;
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
