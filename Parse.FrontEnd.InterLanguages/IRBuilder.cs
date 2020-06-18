using Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.InterLanguages
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

        public abstract IRFormat CreateDclFunction(IROptions options, FuncData funcData);
        public abstract IRFormat CreateDefineFunction(IROptions options, FuncData funcData, IRBlock stmt);
        public abstract IRFormat CreateDclVar(IROptions options, VarData varData, bool bGlobal);
        public abstract IRFormat CreateLoadVar(IROptions options, VarData varData, bool bGlobal);

        #region declare var and initalize
        public abstract IRFormat CreateDclVarAndInit(IROptions options, VarData varData, VarData initInfo, bool bGlobal);
        public abstract IRFormat CreateDclVarAndInit(IROptions options, VarData varData, LiteralData initValue, bool bGlobal);
        #endregion

        #region binary operation
        public abstract IRFormat CreateBinOP(IROptions options, VarData left, VarData right, IROperation operation);
        public abstract IRFormat CreateBinOP(IROptions options, VarData left, LiteralData right, IROperation operation);
        public abstract IRFormat CreateBinOP(IROptions options, LiteralData left, VarData right, IROperation operation);
        public abstract IRFormat CreateBinOP(IROptions options, LiteralData left, LiteralData right, IROperation operation);
        #endregion

        public abstract IRFormat CreatePreInc(IROptions options, VarData varData);
        public abstract IRFormat CreatePreDec(IROptions options, VarData varData);
        public abstract IRFormat CreatePostInc(IROptions options, VarData varData);
        public abstract IRFormat CreatePostDec(IROptions options, VarData varData);

        public abstract IRFormat CreateCall(IROptions options, FuncData funcData, params VarData[] paramDatas);
    }
}
