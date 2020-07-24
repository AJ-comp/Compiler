using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.Properties;

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
        /// This function checks whether "varTokenToCheck" is Declarated.
        /// </summary>
        /// <param name="nodeToCheck"></param>
        /// <param name="varTokenToCheck"></param>
        /// <returns></returns>
        public static bool IsNotDeclared(MiniCNode nodeToCheck, TokenData varTokenToCheck, bool bAttachErrorInfo = true)
        {
            // if it is virtual token, it doesn't need to check duplication.
            if (varTokenToCheck.IsVirtual) return false;

            var varData = MiniCUtilities.GetVarDataFromReferableST(nodeToCheck, varTokenToCheck);
            if (varData != null) return false;

            if (bAttachErrorInfo)
            {
                // Add semantic error information if varData is exist in the SymbolTable.
                nodeToCheck.ConnectedErrInfoList.Add
                    (
                        new MeaningErrInfo(varData.NameToken, 
                                                        nameof(AlarmCodes.MCL0001),
                                                        string.Format(AlarmCodes.MCL0001, varTokenToCheck.Input))
                    );
            }

            return true;
        }


        /// <summary>
        /// This function checks whether the value that not initilized is used.
        /// </summary>
        /// <param name="nodeToCheck"></param>
        /// <param name="varDataToCheck"></param>
        /// <returns></returns>
        public static bool IsUsedNotInitValue(MiniCNode nodeToCheck, MiniCVarData varDataToCheck, bool bAttachErrorInfo = true)
        {
            if (varDataToCheck.IsVirtual) return false;

            var varData = MiniCUtilities.GetVarDataFromReferableST(nodeToCheck, varDataToCheck.NameToken);
            if (varData == null) return false;
            if (varData.Value.IsIncludeNotInit == false) return false;

            if(bAttachErrorInfo)
            {
                // Add semantic error information if varData is exist in the SymbolTable.
                nodeToCheck.ConnectedErrInfoList.Add
                    (
                        new MeaningErrInfo(varData.NameToken, 
                                                        nameof(AlarmCodes.MCL0005),
                                                        string.Format(AlarmCodes.MCL0005, varDataToCheck.Name))
                    );
            }

            return true;
        }
    }
}
