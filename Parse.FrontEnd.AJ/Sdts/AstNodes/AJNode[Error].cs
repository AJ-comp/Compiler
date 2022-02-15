using Parse.FrontEnd.AJ.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public partial class AJNode
    {
        /// <summary>
        /// This function adds symbol is not defined to the node.
        /// </summary>
        /// <param name="varTokenToCheck"></param>
        /// <returns></returns>
        public void AddNotDefinedError(TokenData varTokenToCheck = null)
        {
            Alarms.Add(AJAlarmFactory.CreateMCL0001(varTokenToCheck));
            RootNode.UnLinkedSymbols.Add(this);
        }

        public bool CheckIsDefinedSymbol(TokenData token)
        {
            var symbolData = GetSymbol(token);
            if (symbolData == null) { AddNotDefinedError(token); return false; }
            else if (symbolData.NameToken.IsVirtual) { AddNotDefinedError(token); return false; }

            return true;
        }
    }
}
