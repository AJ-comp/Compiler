using Parse.FrontEnd.Ast;
using static Parse.FrontEnd.Grammars.MiniC.MiniCSymbolItems;

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
        private SymbolTable ActionCommonLogic(TreeNonTerminal node)
        {
            MiniCSymbolTable result = new MiniCSymbolTable();
            MiniCSymbolItems itemInfo = new MiniCSymbolItems();

            foreach (var item in node.Items)
            {
                if (item is TreeNonTerminal)
                {
                    var astNonTerminal = item as TreeNonTerminal;

                    result = result.Merge(astNonTerminal.ActionLogic()) as MiniCSymbolTable;
                }
            }
            return result;
        }

        private SymbolTable ActionProgram(TreeNonTerminal node) => ActionCommonLogic(node);

        private SymbolTable ActionFuncDef(TreeNonTerminal node) => ActionCommonLogic(node);

        private SymbolTable ActionFuncHead(TreeNonTerminal node) => ActionCommonLogic(node);

        private SymbolTable ActionDclSpec(TreeNonTerminal node)
        {
            MiniCSymbolTable result = new MiniCSymbolTable();
            MiniCSymbolItems itemInfo = new MiniCSymbolItems();

            foreach(var item in node.Items)
            {
                if(item is TreeNonTerminal)
                {
                    var astNonTerminal = item as TreeNonTerminal;

                    result = result.Merge(astNonTerminal.ActionLogic()) as MiniCSymbolTable;
                }
                else if (item is TreeTerminal)
                {
                    var astTerminal = item as TreeTerminal;

                    if (astTerminal.Token.Kind == this.grammar.@const) itemInfo.Const = true;
                    else if (astTerminal.Token.Kind == this.grammar.@void) itemInfo.Type = DataType.VOID;
                    else if (astTerminal.Token.Kind == this.grammar.@int) itemInfo.Type = DataType.INT;

                    result.AddItem(itemInfo);
                }
            }
            return result;
        }

        private SymbolTable ActionConstNode(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionIntNode(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionVoidNode(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionFormalPara(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionParamDcl(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionCompoundSt(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionDclList(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionDcl(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionDclItem(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionSimpleVar(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionArrayVar(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionStatList(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionExpSt(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionIfSt(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionIfElseSt(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionWhileSt(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionReturnSt(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionIndex(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionCell(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionActualParam(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionAdd(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionSub(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionMul(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionDiv(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionMod(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionAssign(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionAddAssign(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionSubAssign(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionMulAssign(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionDivAssign(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionModAssign(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionLogicalOr(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionLogicalAnd(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionLogicalNot(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionEqual(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionNotEqual(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionGreaterThan(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionLessThan(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionGreatherEqual(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionLessEqual(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionUnaryMinus(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionPreInc(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionPreDec(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionPostInc(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionPostDec(TreeNonTerminal node)
        {
            return null;
        }


        public MiniCSdts(KeyManager keyManager, MiniCGrammar grammar) : base(keyManager)
        {
            this.grammar = grammar;

            this.Program.ActionLogic = this.ActionProgram;
            this.FuncDef.ActionLogic = this.ActionFuncDef;
            this.FuncHead.ActionLogic = this.ActionFuncHead;
            this.DclSpec.ActionLogic = this.ActionDclSpec;
            this.ConstNode.ActionLogic = this.ActionConstNode;
            this.IntNode.ActionLogic = this.ActionIntNode;
            this.VoidNode.ActionLogic = this.ActionVoidNode;
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
