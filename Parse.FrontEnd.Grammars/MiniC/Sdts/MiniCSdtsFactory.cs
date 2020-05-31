using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.Utilities;
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

        private string NewReservedLabel()
        {
            string labelString;
            do
            {
                labelString = StringUtility.RandomString(5, false);
            } while (_labels.Contains(labelString));
            _labels.Add(labelString);

            return labelString;
        }

        public override event EventHandler<SemanticErrorArgs> SemanticErrorEventHandler;

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
        public MeaningUnit VariableNode { get; } = new MeaningUnit("VariableNode");
        public MeaningUnit IntLiteralNode { get; } = new MeaningUnit("IntLiteralNode");


        public MiniCSdts(KeyManager keyManager, MiniCGrammar grammar) : base(keyManager)
        {
            this.grammar = grammar;

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
            this.Index.BuildLogic = this.BuildIndex;
            this.Call.BuildLogic = this.BuildCallNode;
            this.ActualParam.BuildLogic = this.BuildActualParam;
            this.VariableNode.BuildLogic = this.BuildVarNode;
            this.IntLiteralNode.BuildLogic = this.BuildIntLiteralNode;


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
            this.LogicalOr.BuildLogic = this.BuildOpNode;
            this.LogicalAnd.BuildLogic = this.BuildOpNode;
            this.LogicalNot.BuildLogic = this.BuildOpNode;
            this.Equal.BuildLogic = this.BuildOpNode;
            this.NotEqual.BuildLogic = this.BuildOpNode;
            this.GreaterThan.BuildLogic = this.BuildOpNode;
            this.LessThan.BuildLogic = this.BuildOpNode;
            this.GreaterEqual.BuildLogic = this.BuildOpNode;
            this.LessEqual.BuildLogic = this.BuildOpNode;
            //this.UnaryMinus.BuildLogic = this.ActionUnaryMinus;
            this.PreInc.BuildLogic = this.BuildPreInc;
            this.PreDec.BuildLogic = this.BuildPreDec;
            this.PostInc.BuildLogic = this.BuildPostInc;
            this.PostDec.BuildLogic = this.BuildPostDec;
        }

        public override SemanticAnalysisResult Process(AstSymbol symbol)
        {
            if (symbol == null) return new SemanticAnalysisResult(null, null);
            var buildParams = new MiniCAstBuildParams(new MiniCSymbolTable(), 0, 0);
            var searchedList = new List<AstSymbol>();

            try
            {
                var result = this.BuildProgramNode(symbol as AstNonTerminal, buildParams, searchedList);
                return new SemanticAnalysisResult(symbol, searchedList);
            }
            catch(Exception ex)
            {
                return new SemanticAnalysisResult(symbol, searchedList, ex);
            }
        }
    }
}
