using Parse.FrontEnd.AJ.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public partial class AJNode
    {
        /// <summary>
        /// This function adds duplicated error to the node.
        /// </summary>
        /// <param name="varTokenToCheck"></param>
        /// <returns></returns>
        public bool AddDuplicatedError(TokenData varTokenToCheck = null)
        {
            Alarms.Add(AJAlarmFactory.CreateMCL0001(varTokenToCheck));
            (RootNode as ProgramNode).UnLinkedSymbols.Add(this);

            return true;
        }
    }
}
