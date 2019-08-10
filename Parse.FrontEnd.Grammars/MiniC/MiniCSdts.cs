using Parse.Ast;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class MiniCSdts : Sdts
    {
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

        private void ActionProgram(AstNonTerminal node)
        {

        }

        private void ActionFuncDef(AstNonTerminal node)
        {

        }

        private void ActionFuncHead(AstNonTerminal node)
        {

        }

        private void ActionDclSpec(AstNonTerminal node)
        {

        }

        private void ActionConstNode(AstNonTerminal node)
        {

        }

        private void ActionIntNode(AstNonTerminal node)
        {

        }

        private void ActionVoidNode(AstNonTerminal node)
        {

        }

        private void ActionFormalPara(AstNonTerminal node)
        {

        }

        private void ActionParamDcl(AstNonTerminal node)
        {

        }

        private void ActionCompoundSt(AstNonTerminal node)
        {

        }

        private void ActionDclList(AstNonTerminal node)
        {

        }

        private void ActionDcl(AstNonTerminal node)
        {

        }

        private void ActionDclItem(AstNonTerminal node)
        {

        }

        private void ActionSimpleVar(AstNonTerminal node)
        {

        }

        private void ActionArrayVar(AstNonTerminal node)
        {

        }

        private void ActionStatList(AstNonTerminal node)
        {

        }

        private void ActionExpSt(AstNonTerminal node)
        {

        }

        private void ActionIfSt(AstNonTerminal node)
        {

        }

        private void ActionIfElseSt(AstNonTerminal node)
        {

        }

        private void ActionWhileSt(AstNonTerminal node)
        {

        }

        private void ActionReturnSt(AstNonTerminal node)
        {

        }

        private void ActionIndex(AstNonTerminal node)
        {

        }

        private void ActionCell(AstNonTerminal node)
        {

        }

        private void ActionActualParam(AstNonTerminal node)
        {

        }

        private void ActionAdd(AstNonTerminal node)
        {

        }

        private void ActionSub(AstNonTerminal node)
        {

        }

        private void ActionMul(AstNonTerminal node)
        {

        }

        private void ActionDiv(AstNonTerminal node)
        {

        }

        private void ActionMod(AstNonTerminal node)
        {

        }

        private void ActionAssign(AstNonTerminal node)
        {

        }

        private void ActionAddAssign(AstNonTerminal node)
        {

        }

        private void ActionSubAssign(AstNonTerminal node)
        {

        }

        private void ActionMulAssign(AstNonTerminal node)
        {

        }

        private void ActionDivAssign(AstNonTerminal node)
        {

        }

        private void ActionModAssign(AstNonTerminal node)
        {

        }

        private void ActionLogicalOr(AstNonTerminal node)
        {

        }

        private void ActionLogicalAnd(AstNonTerminal node)
        {

        }

        private void ActionLogicalNot(AstNonTerminal node)
        {

        }

        private void ActionEqual(AstNonTerminal node)
        {

        }

        private void ActionNotEqual(AstNonTerminal node)
        {

        }

        private void ActionGreaterThan(AstNonTerminal node)
        {

        }

        private void ActionLessThan(AstNonTerminal node)
        {

        }

        private void ActionGreatherEqual(AstNonTerminal node)
        {

        }

        private void ActionLessEqual(AstNonTerminal node)
        {

        }

        private void ActionUnaryMinus(AstNonTerminal node)
        {

        }

        private void ActionPreInc(AstNonTerminal node)
        {

        }

        private void ActionPreDec(AstNonTerminal node)
        {

        }

        private void ActionPostInc(AstNonTerminal node)
        {

        }

        private void ActionPostDec(AstNonTerminal node)
        {

        }


        public MiniCSdts(KeyManager keyManager) : base(keyManager)
        {
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
