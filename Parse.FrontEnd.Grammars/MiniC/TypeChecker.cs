using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using Parse.FrontEnd.Grammars.Properties;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class TypeChecker
    {
        #region left type is VarData
        private static MeaningErrInfoList CommonCheck(AstSymbol hs, VarData varData)
        {
            MeaningErrInfoList result = new MeaningErrInfoList();

            if (varData.Value is UnknownLiteralData)
            {
                var convertedLhs = varData.Value as UnknownLiteralData;
                if (convertedLhs.IsOnlyNotInit)
                    result.Add(new MeaningErrInfo(hs, nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, varData.VarName)));
                else if (convertedLhs.IsNotInitAndDynamicAlloc)
                    result.Add(new MeaningErrInfo(hs, nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, varData.VarName), ErrorType.Warning));
            }

            return result;
        }

        private static MeaningErrInfoList CommonCheck(AstNonTerminal exprNode, VarData left, VarData right)
        {
            MeaningErrInfoList result = new MeaningErrInfoList();

            result.AddRange(CommonCheck(exprNode[0], left));
            result.AddRange(CommonCheck(exprNode[1], right));

            return result;
        }

        private static MeaningErrInfoList CommonCheck(AstNonTerminal exprNode, VarData left, LiteralData right)
        {
            MeaningErrInfoList result = new MeaningErrInfoList();

            result.AddRange(CommonCheck(exprNode[0], left));
            result.AddRange(CommonCheck(exprNode[1], right));

            return result;
        }
        #endregion

        #region left type is LiteralData
        private static MeaningErrInfoList CommonCheck(AstSymbol hs, LiteralData literalData)
        {
            MeaningErrInfoList result = new MeaningErrInfoList();

            if (literalData is UnknownLiteralData)
            {
                var convertedLhs = literalData as UnknownLiteralData;
                if (convertedLhs.IsOnlyNotInit)
                    result.Add(new MeaningErrInfo(hs, nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, literalData.LiteralName)));
                else if (convertedLhs.IsNotInitAndDynamicAlloc)
                    result.Add(new MeaningErrInfo(hs, nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, literalData.LiteralName), ErrorType.Warning));
            }

            return result;
        }

        private static MeaningErrInfoList CommonCheck(AstNonTerminal exprNode, LiteralData left, VarData right)
        {
            MeaningErrInfoList result = new MeaningErrInfoList();

            result.AddRange(CommonCheck(exprNode[0], left));
            result.AddRange(CommonCheck(exprNode[1], right));

            return result;
        }

        private static MeaningErrInfoList CommonCheck(AstNonTerminal exprNode, LiteralData left, LiteralData right)
        {
            MeaningErrInfoList result = new MeaningErrInfoList();

            result.AddRange(CommonCheck(exprNode[0], left));
            result.AddRange(CommonCheck(exprNode[1], right));

            return result;
        }
        #endregion

        public static MeaningErrInfoList Check(AstNonTerminal exprNode, VarData left, object right)
        {
            MeaningErrInfoList result = new MeaningErrInfoList();

            if (right is VarData)
                result.AddRange(TypeChecker.CommonCheck(exprNode, left, right as VarData));
            else if(right is LiteralData)
                result.AddRange(TypeChecker.CommonCheck(exprNode, left, right as LiteralData));

            return result;
        }

        public static MeaningErrInfoList Check(AstNonTerminal exprNode, LiteralData left, object right)
        {
            MeaningErrInfoList result = new MeaningErrInfoList();

            if (right is VarData)
                result.AddRange(TypeChecker.CommonCheck(exprNode, left, right as VarData));
            else if (right is LiteralData)
                result.AddRange(TypeChecker.CommonCheck(exprNode, left, right as LiteralData));

            return result;
        }

        //public static MeaningErrInfoList Check(AstNonTerminal exprNode, object left, object right)
        //{
        //    MeaningErrInfoList result = new MeaningErrInfoList();

        //    if (right is VarData)
        //        result.AddRange(TypeChecker.CommonCheck(exprNode, left, right as VarData));
        //    else if (right is LiteralData)
        //        result.AddRange(TypeChecker.CommonCheck(exprNode, left, right as LiteralData));

        //    return result;
        //}
    }
}
