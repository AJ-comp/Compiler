using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR
{
    public abstract class SSATable : ICloneable<SSATable>
    {
        public IEnumerable<RootChainVar> GlobalRootChainVars => _globalRootChainVars;
        public IEnumerable<RootChainVar> LocalRootChainVars => _localRootChainVars;

        public SSATable()
        {
        }


        public RootChainVar Find(IRVar toFind)
        {
            var result = FindFromLocalList(toFind);
            if (result == null) result = FindFromGlobalList(toFind);

            return result;
        }

        public RootChainVar FindFromGlobalList(IRVar toFind) => FindCore(toFind, GlobalRootChainVars);
        public RootChainVar FindFromLocalList(IRVar toFind) => FindCore(toFind, LocalRootChainVars);

        private RootChainVar FindCore(IRVar toFind, IEnumerable<RootChainVar> rootVarList)
        {
            RootChainVar result = null;

            foreach (var rootVar in rootVarList)
            {
                if (rootVar.Name != toFind.Name) continue;

                result = rootVar;
                break;
            }

            return result;
        }

        public abstract SSATable Clone();

        protected List<RootChainVar> _globalRootChainVars = new List<RootChainVar>();
        protected List<RootChainVar> _localRootChainVars = new List<RootChainVar>();
    }
}
