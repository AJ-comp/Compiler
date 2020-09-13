using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.Properties;
using Parse.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts : Grammars.Sdts
    {
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
        public MeaningUnit DefinePrep { get; } = new MeaningUnit("DefinePrep");
        public MeaningUnit FuncDef { get; } = new MeaningUnit("FuncDef");
        public MeaningUnit FuncHead { get; } = new MeaningUnit("FuncHead");
        public MeaningUnit DclSpec { get; } = new MeaningUnit("DclSpec");
        public MeaningUnit ConstNode { get; } = new MeaningUnit("ConstNode");
        public MeaningUnit CharNode { get; } = new MeaningUnit("CharNode");
        public MeaningUnit ShortNode { get; } = new MeaningUnit("ShortNode");
        public MeaningUnit IntNode { get; } = new MeaningUnit("IntNode");
        public MeaningUnit DoubleNode { get; } = new MeaningUnit("DoubleNode");
        public MeaningUnit VoidNode { get; } = new MeaningUnit("VoidNode");
        public MeaningUnit FormalPara { get; } = new MeaningUnit("FormalPara");
        public MeaningUnit ParamDcl { get; } = new MeaningUnit("ParamDcl");
        public MeaningUnit CompoundSt { get; } = new MeaningUnit("CompoundSt");
        public MeaningUnit DclList { get; } = new MeaningUnit("DclList");
        public MeaningUnit Dcl { get; } = new MeaningUnit("Dcl");
        public MeaningUnit DclItem { get; } = new MeaningUnit("DclItem");
        public MeaningUnit DeclareVar { get; } = new MeaningUnit("DeclareVar");
        public MeaningUnit StatList { get; } = new MeaningUnit("StatList");
        public MeaningUnit ExpSt { get; } = new MeaningUnit("ExpSt");
        public MeaningUnit IfSt { get; } = new MeaningUnit("IfSt");
        public MeaningUnit IfElseSt { get; } = new MeaningUnit("IfElseSt");
        public MeaningUnit WhileSt { get; } = new MeaningUnit("WhileSt");
        public MeaningUnit ReturnSt { get; } = new MeaningUnit("ReturnSt");
        public MeaningUnit Index { get; } = new MeaningUnit("Index");
        public MeaningUnit Call { get; } = new MeaningUnit("Call");
        public MeaningUnit ActualParam { get; } = new MeaningUnit("ActualParam");
        public MeaningUnit UseVar { get; } = new MeaningUnit("UseVar");
        public MeaningUnit IntLiteralNode { get; } = new MeaningUnit("IntLiteralNode");


        private Hashtable _sdtsFinder = new Hashtable();
        // The cache role for speed up
        private MiniCGrammar _grammar;
        private StringCollection _labels = new StringCollection();
        private string _reservedLabel = string.Empty;


        public MiniCSdts(KeyManager keyManager, MiniCGrammar grammar) : base(keyManager)
        {
            this._grammar = grammar;
        }

        public override SemanticAnalysisResult Process(AstSymbol symbol)
        {
            if (symbol == null) return new SemanticAnalysisResult(null, null);

            SdtsNode result = null;
            var searchedList = new List<AstSymbol>();

            try
            {
                var sdtsRoot = GenerateSdtsAst(symbol);
                result = sdtsRoot.Build(new MiniCSdtsParams(0, 0));
                return new SemanticAnalysisResult(result, searchedList);
            }
            catch(Exception ex)
            {
                return new SemanticAnalysisResult(result, searchedList, ex);
            }
        }

        public override SdtsNode GenerateSdtsAst(AstSymbol root)
        {
            MiniCNode result = null;

            if (root is AstTerminal) result = new TerminalNode(root);
            else
            {
                AstNonTerminal cRoot = root as AstNonTerminal;

                if (cRoot.SignPost.MeaningUnit == Program) result = new ProgramNode(root);
                else if (cRoot.SignPost.MeaningUnit == DefinePrep) result = new DefinePrepNode(root);
                else if (cRoot.SignPost.MeaningUnit == FuncDef) result = new FuncDefNode(root);
                else if (cRoot.SignPost.MeaningUnit == FuncHead) result = new FuncHeadNode(root);
                else if (cRoot.SignPost.MeaningUnit == FormalPara) result = new ParamListNode(root);
                else if (cRoot.SignPost.MeaningUnit == ParamDcl) result = new ParamNode(root);
                else if (cRoot.SignPost.MeaningUnit == CompoundSt) result = new CompoundStNode(root);
                else if (cRoot.SignPost.MeaningUnit == StatList) result = new StatListNode(root);
                else if (cRoot.SignPost.MeaningUnit == IfSt) result = new IfStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == IfElseSt) result = new IfElseStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == WhileSt) result = new WhileStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == ReturnSt) result = new ReturnStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == ExpSt) result = new ExprStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == Call) result = new CallNode(root);
                else if (cRoot.SignPost.MeaningUnit == ActualParam) result = new ActualParamNode(root);
                else if (cRoot.SignPost.MeaningUnit == Index) result = new IndexNode(root);

                else if (cRoot.SignPost.MeaningUnit == Add) result = new AddExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == Sub) result = new SubExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == Mul) result = new MulExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == Div) result = new DivExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == Mod) result = new ModExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == Assign) result = new AssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == AddAssign) result = new AddAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == SubAssign) result = new SubAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MulAssign) result = new MulAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == DivAssign) result = new DivAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == ModAssign) result = new ModAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == PreInc) result = new PreIncExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == PreDec) result = new PreDecExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == PostInc) result = new PostIncExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == PostDec) result = new PostDecExprNode(root);

                else if (cRoot.SignPost.MeaningUnit == LogicalOr) result = new OrExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == LogicalAnd) result = new AndExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == LogicalNot) result = new NotExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == Equal) result = new EqualExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == NotEqual) result = new NotEqualExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == GreaterThan) result = new GreaterThanNode(root);
                else if (cRoot.SignPost.MeaningUnit == LessThan) result = new LessThanNode(root);
                else if (cRoot.SignPost.MeaningUnit == GreaterEqual) result = new GreaterEqualNode(root);
                else if (cRoot.SignPost.MeaningUnit == LessEqual) result = new LessEqualNode(root);

                else if (cRoot.SignPost.MeaningUnit == DclSpec) result = new VariableTypeNode(root);
                else if (cRoot.SignPost.MeaningUnit == ConstNode) result = new ConstNode(root);
                else if (cRoot.SignPost.MeaningUnit == VoidNode) result = new VoidNode(root);
                else if (cRoot.SignPost.MeaningUnit == IntNode) result = new IntNode(root);
                else if (cRoot.SignPost.MeaningUnit == DclList) result = new VariableDclsListNode(root);
                else if (cRoot.SignPost.MeaningUnit == Dcl) result = new VariableDclsNode(root);
                else if (cRoot.SignPost.MeaningUnit == DclItem) result = new InitDeclaratorNode(root);
                else if (cRoot.SignPost.MeaningUnit == DeclareVar) result = new DeclareVarNode(root);
                else if (cRoot.SignPost.MeaningUnit == UseVar) result = new UseIdentNode(root);
                else if (cRoot.SignPost.MeaningUnit == IntLiteralNode) result = new IntLiteralNode(root);
                else throw new Exception(AlarmCodes.MCL0010);

                foreach (var item in cRoot.Items)
                {
                    var childNode = GenerateSdtsAst(item);
                    if (childNode == null) return null;

                    childNode.Parent = result;
                    result.Items.Add(childNode);
                }
            }

            return result;
        }
    }
}
