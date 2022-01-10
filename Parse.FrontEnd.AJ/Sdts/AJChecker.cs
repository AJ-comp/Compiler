using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using System;
using System.Threading.Tasks;

namespace Parse.FrontEnd.AJ.Sdts
{
    public class AJChecker
    {
        public static bool IsDefinedSymbol(AJNode node, TokenData token)
        {
            bool result = false;
            var variable = node.GetSymbol(token);

            if (variable == null)
            {
                result = false;
                node.ConnectedErrInfoList.Add(new MeaningErrInfo(nameof(Properties.AlarmCodes.MCL0001),
                                                                                            string.Format(Properties.AlarmCodes.MCL0001, token.Input)));
            }
            else
            {
                result = true;
            }

            return result;
        }
    }
}
