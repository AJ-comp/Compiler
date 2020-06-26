using Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.InterLanguages.UCode
{
    public enum OpCodeKind
    {
        sym, lod, lda, ldc, proc, end, call, ret,
        pop, str, sti, ldi,
        add, sub, mult, div, mod,
        swp, and, or, gt, lt, ge, le, eq, ne, not, neg,
        inc, dec,
        ujp, tjp, fjp,
        dup, nop,
    };

    public class UCodeBuilder : IRBuilder
    {
        public override bool IsSSA => false;

        public override IRFormat CreateAnd(IROptions options, IRData left, IRData right)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateBinOP(IROptions options, IRData left, IRData right, IROperation operation)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateCall(IROptions options, IRFuncData funcData, params IRVar[] paramDatas)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateDclFunction(IROptions options, IRFuncData funcData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateDclVar(IROptions options, IRVar varData, bool bGlobal)
        {
            return new IRFormat(Command.DclVar(options.Label, varData.Block, varData.Offset, varData.Length, options.Comment));
        }

        public override IRFormat CreateDclVarAndInit(IROptions options, IRVar varData, IRVar initInfo, bool bGlobal)
        {
            var block = new IRBlock()
            {
                Command.DclVar(options.Label, varData.Block, varData.Offset, varData.Length, options.Comment),
                Command.DclVar("", varData.Block, varData.Offset, varData.Length, options.Comment)
            };

            return new IRFormat(block);
        }

        public override IRFormat CreateDclVarAndInit(IROptions options, IRVar VarData, IRLiteral initValue, bool bGlobal)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateDefineFunction(IROptions options, IRFuncData funcData, IRBlock stmt)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateLoadVar(IROptions options, IRVar VarData, bool bGlobal)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateLogicalOp(IROptions options, IRData left, IRData right, IRCondition cond)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateNot(IROptions options, IRData data)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateOr(IROptions options, IRData left, IRData right)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreatePostDec(IROptions options, IRVar varData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreatePostInc(IROptions options, IRVar varData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreatePreDec(IROptions options, IRVar varData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreatePreInc(IROptions options, IRVar varData)
        {
            throw new System.NotImplementedException();
        }

        public class Command
        {
            private static Instruction UCodeFormat(string labelName, OpCodeKind opCode, string comment = "", params object[] operands)
                => new Instruction(labelName, opCode, comment, operands);

            public static Instruction DclVar(string labelName, int bIndex, int oIndex, int length, string comment = "")
                => UCodeFormat(labelName, OpCodeKind.sym, comment, bIndex, oIndex, length);

            public static Instruction LoadVarValue(string labelName, int bIndex, int oIndex, string comment = "")
                => UCodeFormat(labelName, OpCodeKind.lod, comment, bIndex, oIndex);

            public static Instruction LoadVarAddress(string labelName, int bIndex, int oIndex, string comment = "")
                => UCodeFormat(labelName, OpCodeKind.lda, comment, bIndex, oIndex);

            public static Instruction DclValue(string labelName, int value, string comment = "")
                => UCodeFormat(labelName, OpCodeKind.ldc, comment, value);

            public static Instruction ProcStart(string procName, int totalLength, int bIndex, string comment = "")
                => UCodeFormat(procName, OpCodeKind.proc, comment, totalLength, bIndex, 2);

            public static Instruction ProcEnd(string labelName, string comment = "")
                => UCodeFormat(labelName, OpCodeKind.end, comment);

            public static Instruction ProcCall(string labelName, string procName, params IRParamVar[] param)
                => UCodeFormat(labelName, OpCodeKind.call, procName, procName);

            public static Instruction RetFromProc(string labelName, string comment = "")
                => UCodeFormat(labelName, OpCodeKind.ret, comment);

            public static Instruction UnconditionalJump(string labelName, string destLableName, string comment = "")
                => UCodeFormat(labelName, OpCodeKind.ujp, comment, destLableName);

            public static Instruction ConditionalJump(string labelName, string destLabelName, bool bTrue = true, string comment = "")
            {
                var command = (bTrue) ? OpCodeKind.tjp : OpCodeKind.fjp;

                return UCodeFormat(labelName, command, comment, destLabelName);
            }

            public static Instruction Pop(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.pop, comment);
            public static Instruction Store(string labelName, int bIndex, int oIndex, string comment = "") => UCodeFormat(labelName, OpCodeKind.str, comment, bIndex, oIndex);
            public static Instruction Sti(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.sti, comment);
            public static Instruction Ldi(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.ldi, comment);
            public static Instruction Add(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.add, comment);
            public static Instruction Sub(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.sub, comment);
            public static Instruction Multiple(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.mult, comment);
            public static Instruction Div(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.div, comment);
            public static Instruction Mod(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.mod, comment);
            public static Instruction Swap(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.swp, comment);
            public static Instruction And(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.and, comment);
            public static Instruction Or(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.or, comment);
            public static Instruction GreaterThan(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.gt, comment);
            public static Instruction LessThan(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.lt, comment);
            public static Instruction GreaterEqual(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.ge, comment);
            public static Instruction LessEqual(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.le, comment);
            public static Instruction Equal(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.eq, comment);
            public static Instruction NegativeEqual(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.ne, comment);
            public static Instruction Not(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.not, comment);
            public static Instruction Negative(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.neg, comment);
            public static Instruction Increment(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.inc, comment);
            public static Instruction Decrement(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.dec, comment);
            public static Instruction Duplicate(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.dup, comment);
            public static Instruction NoOperate(string labelName, string comment = "") => UCodeFormat(labelName, OpCodeKind.nop, comment);
        }
    }
}
