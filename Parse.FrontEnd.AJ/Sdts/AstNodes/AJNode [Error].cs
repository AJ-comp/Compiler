using Parse.FrontEnd.AJ.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract partial class AJNode
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

        public void AddUnExpectedError(string message)
        {
            Alarms.Add(new MeaningErrInfo(AllTokens,
                                                            nameof(AlarmCodes.AJ9998),
                                                            string.Format(AlarmCodes.AJ9998, GetType().Name)));
        }

        public void AddAlarmNoSymbolInContext()
        {
            Alarms.Add(new MeaningErrInfo(AllTokens,
                                                            nameof(AlarmCodes.AJ0047),
                                                            string.Format(AlarmCodes.AJ0047, AllTokens.Last().Input)));
        }

        public bool CheckIsDefinedSymbol(TokenData token)
        {
            var symbolData = GetSymbol(token);
            if (symbolData == null) { AddNotDefinedError(token); return false; }
            else if (symbolData.NameToken.IsVirtual) { AddNotDefinedError(token); return false; }

            return true;
        }

        public bool CheckIsDefinedSymbolChain(IEnumerable<TokenData> tokens)
        {
            foreach (var token in tokens)
            {
                var symbolData = GetSymbol(token);
                if (symbolData == null) { AddNotDefinedError(token); return false; }
                else if (symbolData.NameToken.IsVirtual) { AddNotDefinedError(token); return false; }
            }

            return true;
        }
    }
}
