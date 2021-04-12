using Parse.FrontEnd.MiniC.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public partial class MiniCNode
    {
        /// <summary>
        /// This function adds duplicated error to the node.
        /// </summary>
        /// <param name="varTokenToCheck"></param>
        /// <returns></returns>
        public bool AddDuplicatedError(TokenData varTokenToCheck = null)
        {
            if (varTokenToCheck == null)
            {
                ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(AllTokens,
                                                    nameof(AlarmCodes.MCL0009),
                                                    string.Format(AlarmCodes.MCL0009, AllTokens[0].Input))
                );
            }
            else
            {
                ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(varTokenToCheck,
                                                    nameof(AlarmCodes.MCL0009),
                                                    string.Format(AlarmCodes.MCL0009, varTokenToCheck.Input))
                );
            }

            return true;
        }
    }
}
