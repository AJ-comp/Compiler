using Parse.FrontEnd.Ast;
using System;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class MiniCSdts : Sdts
    {
        // The cache role for speed up
        private MiniCGrammar grammar;

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

        /// <summary>
        /// This function define common logic if node included items that only TreeNonTerminal type.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private object ActionCommonLogic(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            bool result = true;

            foreach (var item in node.Items)
            {
                if ((item is TreeNonTerminal) == false) return false;

                var astNonTerminal = item as TreeNonTerminal;
                if ((bool)astNonTerminal.ActionLogic(table, parsingInfo, errList) == false) result = false;
            }

            return result;
        }

        private object ActionProgram(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            bool result = true;

            foreach (var item in node.Items)
            {
                if ((item is TreeNonTerminal) == false) return false;

                var astNonTerminal = item as TreeNonTerminal;
                if ((bool)astNonTerminal.ActionLogic(table, parsingInfo, errList) == false) result = false;
            }

            return result;
        }

        private object ActionFuncDef(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
            => ActionCommonLogic(node, table, parsingInfo, errList);

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
        private object ActionFuncHead(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            foreach(var item in node.Items)
            {
                // ident
                if(item is TreeTerminal)
                {
                    var token = item as TreeTerminal;

//                    token.Token.Input
                }
            }

            return false;
        }

        private object ActionDclSpec(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            bool result = true;
            parsingInfo.SnippetToAdd = new MiniCVarInfoSnippet();

            foreach (var item in node.Items)
            {
                if ((item is TreeNonTerminal) == false) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal.signPost.MeaningUnit == this.ConstNode) 
                { 
                    parsingInfo.IncTokenIndex();

                    var snippet = parsingInfo.SnippetToAdd as MiniCVarInfoSnippet;
                    snippet.Const = true; 
                }
                else if (astNonTerminal.signPost.MeaningUnit == this.VoidNode) 
                { 
                    parsingInfo.IncTokenIndex();

                    var snippet = parsingInfo.SnippetToAdd as MiniCVarInfoSnippet;
                    snippet.Type = DataType.VOID; 
                }
                else if (astNonTerminal.signPost.MeaningUnit == this.IntNode) 
                { 
                    parsingInfo.IncTokenIndex();

                    var snippet = parsingInfo.SnippetToAdd as MiniCVarInfoSnippet;
                    snippet.Type = DataType.INT; 
                }
            }

            return result;
        }

        private object ActionFormalPara(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionParamDcl(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionCompoundSt(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return false;
        }

        private object ActionDclList(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionDcl(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList) 
            => ActionCommonLogic(node, table, parsingInfo, errList);

        private object ActionDclItem(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return false;
        }

        private object ActionSimpleVar(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            if (node.Items.Count == 0)
            {
                parsingInfo.SnippetToAdd = null;
                return false;
            }

            var item = node.Items[0] as TreeNonTerminal;

            return true;
        }

        private object ActionArrayVar(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionStatList(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionExpSt(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionIfSt(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionIfElseSt(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionWhileSt(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionReturnSt(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionIndex(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionCell(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionActualParam(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionAdd(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionSub(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionMul(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionDiv(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionMod(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionAssign(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionAddAssign(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionSubAssign(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionMulAssign(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionDivAssign(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionModAssign(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLogicalOr(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLogicalAnd(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLogicalNot(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionEqual(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionNotEqual(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionGreaterThan(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLessThan(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionGreatherEqual(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionLessEqual(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionUnaryMinus(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionPreInc(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionPreDec(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionPostInc(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        private object ActionPostDec(TreeNonTerminal node, SymbolTable table, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList)
        {
            return null;
        }

        public override MeaningAnalysisResult Process(TreeSymbol symbol)
        {
            MeaningErrInfoList errList = new MeaningErrInfoList();
            MiniCSymbolTable table = new MiniCSymbolTable();

            this.ActionProgram(symbol as TreeNonTerminal, table, new MeaningParsingInfo(), errList);
            return new MeaningAnalysisResult(errList, table);
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
