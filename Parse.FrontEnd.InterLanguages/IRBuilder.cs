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

        public abstract IRFormat CreateDclFunction(IROptions options, IRFuncData funcData);
        public abstract IRFormat CreateDefineFunction(IROptions options, IRFuncData funcData, IRBlock stmt);
        public abstract IRFormat CreateDclVar(IROptions options, IRVarData varData, bool bGlobal);
        public abstract IRFormat CreateLoadVar(IROptions options, IRVarData varData, bool bGlobal);

        #region declare var and initalize
        public abstract IRFormat CreateDclVarAndInit(IROptions options, IRVarData varData, IRVarData initInfo, bool bGlobal);
        public abstract IRFormat CreateDclVarAndInit(IROptions options, IRVarData varData, IRLiteralData initValue, bool bGlobal);
        #endregion

        #region binary operation
        public abstract IRFormat CreateBinOP(IROptions options, IRData left, IRData right, IROperation operation);
        #endregion

        public abstract IRFormat CreatePreInc(IROptions options, IRVarData varData);
        public abstract IRFormat CreatePreDec(IROptions options, IRVarData varData);
        public abstract IRFormat CreatePostInc(IROptions options, IRVarData varData);
        public abstract IRFormat CreatePostDec(IROptions options, IRVarData varData);

        public abstract IRFormat CreateCall(IROptions options, IRFuncData funcData, params IRVarData[] paramDatas);
    }
}
