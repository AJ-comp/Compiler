using Parse.FrontEnd.InterLanguages.Datas.Types;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.ValueDatas;

namespace Parse.MiddleEnd.IR
{
    /// <summary>
    /// Strategy Pattern
    /// </summary>
    public abstract class IRBuilder
    {
        /// <summary>
        /// This property means whether SSA format is.
        /// </summary>
        public abstract bool IsSSA { get; }

        public abstract IRFormat CreateDclFunction(IROptions options, IRFuncData funcData);
        public abstract IRFormat CreateDefineFunction(IROptions options, IRFuncData funcData, IRBlock stmt);
        public abstract IRFormat CreateDclVar(IROptions options, IRVar varData, bool bGlobal);
        public abstract IRFormat CreateLoadVar(IROptions options, IRVar varData, bool bGlobal);

        #region declare var and initalize
        public abstract IRFormat CreateDclVarAndInit(IROptions options, IRVar varData, IRVar initInfo, bool bGlobal);
        public abstract IRFormat CreateDclVarAndInit(IROptions options, IRVar varData, IRValue initValue, bool bGlobal);
        #endregion

        #region binary operation
        public abstract IRFormat CreateBinOP(IROptions options, IRData left, IRData right, IROperation operation);
        #endregion

        public abstract IRFormat CreatePreInc(IROptions options, IRVar varData);
        public abstract IRFormat CreatePreDec(IROptions options, IRVar varData);
        public abstract IRFormat CreatePostInc(IROptions options, IRVar varData);
        public abstract IRFormat CreatePostDec(IROptions options, IRVar varData);

        public abstract IRFormat CreateNot(IROptions options, IRData data);
        public abstract IRFormat CreateAnd(IROptions options, IRValue<Bit> left, IRValue<Bit> right);
        public abstract IRFormat CreateOr(IROptions options, IRValue<Bit> left, IRValue<Bit> right);
        public abstract IRFormat CreateLogicalOp(IROptions options, IRData left, IRData right, IRCondition cond);


        public abstract IRFormat CreateCall(IROptions options, IRFuncData funcData, params IRVar[] paramDatas);
    }
}
