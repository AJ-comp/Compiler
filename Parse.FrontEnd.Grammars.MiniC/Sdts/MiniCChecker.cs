using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.Grammars.Properties;
using static Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables.VariableMiniC;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public class MiniCChecker
    {
        /// <summary>
        /// This function checks whether "varTokenToCheck" is duplicated.
        /// </summary>
        /// <param name="nodeToCheck"></param>
        /// <param name="param"></param>
        /// <param name="varTokenToCheck"></param>
        /// <returns></returns>
       public static bool IsDuplicated(MiniCNode nodeToCheck, SdtsParams param, TokenData varTokenToCheck)
        {
            // if it is virtual token, it doesn't need to check duplication.
            if (varTokenToCheck.IsVirtual) return false;

            // check duplication
            var varData = (param as MiniCSdtsParams).SymbolTable.GetVarByName(varTokenToCheck.Input);
            if (varData == null) return false;

            // Add semantic error information if varData is not exist in the SymbolTable.
            nodeToCheck.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(varData.NameToken, 
                                                    nameof(AlarmCodes.MCL0009), 
                                                    string.Format(AlarmCodes.MCL0009, varTokenToCheck.Input))
                );

            return true;
        }


        /// <summary>
        /// This function checks whether the value that not initilized is used.
        /// </summary>
        /// <param name="nodeToCheck"></param>
        /// <param name="varDataToCheck"></param>
        /// <returns></returns>
        public static bool IsUsedNotInitValue(MiniCNode nodeToCheck, VariableMiniC varDataToCheck, bool bAttachErrorInfo = true)
        {
            if (varDataToCheck.IsVirtual) return false;

            var varRecord = MiniCUtilities.GetVarRecordFromReferableST(nodeToCheck, varDataToCheck.NameToken);
            if (varRecord == null) return false;
            if (varRecord.InitValue != null) return false;

            if(bAttachErrorInfo)
            {
                // Add semantic error information if varData is exist in the SymbolTable.
                nodeToCheck.ConnectedErrInfoList.Add
                    (
                        new MeaningErrInfo(varRecord.VarField.NameToken, 
                                                        nameof(AlarmCodes.MCL0005),
                                                        string.Format(AlarmCodes.MCL0005, varDataToCheck.Name))
                    );
            }

            return true;
        }


        public static bool IsAllArithmetic(MiniCDataType left, MiniCDataType right)
        {
            return (left == MiniCDataType.Int && right == MiniCDataType.Int);
        }
    }
}
