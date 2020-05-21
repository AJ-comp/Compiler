using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts : Grammars.Sdts
    {
        // The cache role for speed up
        private MiniCGrammar grammar;
        private StringCollection _labels = new StringCollection();
        private string _reservedLabel = string.Empty;

        // Auto clear when a reference
        public string ReservedLabel
        {
            get
            {
                string result = _reservedLabel;

                _reservedLabel = string.Empty;
                return result;
            }
            private set => _reservedLabel = value;
        }

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
        public MeaningUnit Call { get; } = new MeaningUnit("Call");
        public MeaningUnit ActualParam { get; } = new MeaningUnit("ActualParam");


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
            this.Call.ActionLogic = this.ActionCall;
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
            this.GreaterEqual.ActionLogic = this.ActionGreatherEqual;
            this.LessEqual.ActionLogic = this.ActionLessEqual;
            this.UnaryMinus.ActionLogic = this.ActionUnaryMinus;
            this.PreInc.ActionLogic = this.ActionPreInc;
            this.PreDec.ActionLogic = this.ActionPreDec;
            this.PostInc.ActionLogic = this.ActionPostInc;
            this.PostDec.ActionLogic = this.ActionPostDec;



            this.Program.BuildLogic = this.BuildProgramNode;
            this.FuncDef.BuildLogic = this.BuildFuncDefNode;
            this.FuncHead.BuildLogic = this.BuildFuncHeadNode;
            this.DclSpec.BuildLogic = this.BuildDclSpecNode;
            this.FormalPara.BuildLogic = this.BuildFormalParaNode;
            this.ParamDcl.BuildLogic = this.BuildParamDcl;
            this.CompoundSt.BuildLogic = this.BuildCompoundStNode;
            this.DclList.BuildLogic = this.BuildDclListNode;
            this.Dcl.BuildLogic = this.BuildDclNode;
            this.DclItem.BuildLogic = this.BuildDclItemNode;
            this.SimpleVar.BuildLogic = this.BuildSimpleVarNode;
            this.ArrayVar.BuildLogic = this.BuildArrayVarNode;
            this.StatList.BuildLogic = this.BuildStatListNode;
            this.ExpSt.BuildLogic = this.BuildExpStNode;
            this.IfSt.BuildLogic = this.BuildIfStNode;
            this.IfElseSt.BuildLogic = this.BuildIfElseStNode;
            this.WhileSt.BuildLogic = this.BuildWhileStNode;
            this.ReturnSt.BuildLogic = this.BuildReturnStNode;
//            this.Index.BuildLogic = this.buildin;
            this.Call.BuildLogic = this.BuildCallNode;
            this.ActualParam.BuildLogic = this.BuildActualParam;

            this.Add.BuildLogic = this.BuildAdd;
            this.Sub.BuildLogic = this.BuildSub;
            this.Mul.BuildLogic = this.BuildMul;
            this.Div.BuildLogic = this.BuildDiv;
            this.Mod.BuildLogic = this.BuildMod;
            this.Assign.BuildLogic = this.BuildAssign;
            this.AddAssign.BuildLogic = this.BuildAddAssign;
            this.SubAssign.BuildLogic = this.BuildSubAssign;
            this.MulAssign.BuildLogic = this.BuildMulAssign;
            this.DivAssign.BuildLogic = this.BuildDivAssign;
            this.ModAssign.BuildLogic = this.BuildModAssign;
            this.LogicalOr.BuildLogic = this.BuildExpressionNode;
            this.LogicalAnd.BuildLogic = this.BuildExpressionNode;
            this.LogicalNot.BuildLogic = this.BuildExpressionNode;
            this.Equal.BuildLogic = this.BuildExpressionNode;
            this.NotEqual.BuildLogic = this.BuildExpressionNode;
            this.GreaterThan.BuildLogic = this.BuildExpressionNode;
            this.LessThan.BuildLogic = this.BuildExpressionNode;
            this.GreaterEqual.BuildLogic = this.BuildExpressionNode;
            this.LessEqual.BuildLogic = this.BuildExpressionNode;
            //this.UnaryMinus.BuildLogic = this.ActionUnaryMinus;
            //this.PreInc.BuildLogic = this.ActionPreInc;
            //this.PreDec.BuildLogic = this.ActionPreDec;
            //this.PostInc.BuildLogic = this.ActionPostInc;
            //this.PostDec.BuildLogic = this.ActionPostDec;
        }

        public override SementicAnalysisResult Process(AstSymbol symbol)
        {
            MeaningErrInfoList errList = new MeaningErrInfoList();
            if (symbol == null) return new SementicAnalysisResult(errList, null);

            try
            {
                this.BuildProgramNode(symbol as AstNonTerminal, new MiniCSymbolTable(), 0, 0);
                return new SementicAnalysisResult(errList, null);
            }
            catch
            {
                return null;
            }
        }

        public override IReadOnlyList<AstSymbol> GenerateCode(AstSymbol symbol)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            try
            {
                if (symbol != null)
                {
                    result = this.ActionProgram(symbol as AstNonTerminal) as List<AstSymbol>;
                }
            }
            catch
            {
                return null;
            }

            return result;
        }
    }
}
