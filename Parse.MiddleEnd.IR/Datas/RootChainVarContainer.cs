using System.Diagnostics;

namespace Parse.MiddleEnd.IR.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class RootChainVarContainer : UseDefChainVar
    {
        public SSAVar LinkedObject { get; private set; }

        public override string Name => VarInfo.Name;

        public RootChainVarContainer(IRDeclareVar var) : base(var)
        {
        }

        public override void Link(SSAVar toLinkObject)
        {
            LinkedObject = toLinkObject;
        }

        /// <summary>
        /// This function returns new IRVar type that Name property modified to nameToChange.
        /// This function used to create RootChainVar to save param value.
        /// </summary>
        /// <param name="var"></param>
        /// <param name="nameToChange"></param>
        /// <returns></returns>
        /*
        public static IRVar Copy(IRVar var, string nameToChange)
        {
            var result = new RootChainVar(var);
            result._name = nameToChange;

            return result;
        }
        */


        public override string DebuggerDisplay
        {
            get
            {
                var result = base.DebuggerDisplay;

                if (LinkedObject != null)
                    result += string.Format(" -> {0}", LinkedObject.DebuggerDisplay);

                return result;
            }
        }
    }
}
