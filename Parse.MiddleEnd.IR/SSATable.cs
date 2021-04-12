using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR
{
    public abstract class SSATable : ICloneable<SSATable>
    {
        public IEnumerable<RootChainVarContainer> GlobalRootChainVars => _globalRootChainVars;
        public IEnumerable<RootChainVarContainer> LocalRootChainVars => _localRootChainVars;

        public SSATable()
        {
        }


        public RootChainVarContainer Find(IRDeclareVar toFind)
        {
            var result = FindFromLocalList(toFind);
            if (result == null) result = FindFromGlobalList(toFind);

            return result;
        }

        public SSAVar FindMemberVar(IRUseMemberVarExpr toFind)
        {
            // Try to find root info for struct
            var structDefInfo = IRUserDefTypeList.Instance[toFind.StructVar.Name];

            // Get memer var of struct
            return FindFromStruct(structDefInfo, toFind.Offset);
        }

        public RootChainVarContainer FindFromGlobalList(IRDeclareVar toFind) => FindCore(toFind, GlobalRootChainVars);
        public RootChainVarContainer FindFromLocalList(IRDeclareVar toFind) => FindCore(toFind, LocalRootChainVars);
        public SSAVar FindFromStruct(IRStructDefInfo from, int offset) => from.MemberVarList.ElementAt(offset) as SSAVar;

        private RootChainVarContainer FindCore(IRDeclareVar toFind, IEnumerable<RootChainVarContainer> rootVarList)
        {
            RootChainVarContainer result = null;

            foreach (var rootVar in rootVarList)
            {
                if (rootVar.Name != toFind.Name) continue;

                result = rootVar;
                break;
            }

            return result;
        }

        public abstract SSATable Clone();

        protected List<RootChainVarContainer> _globalRootChainVars = new List<RootChainVarContainer>();
        protected List<RootChainVarContainer> _localRootChainVars = new List<RootChainVarContainer>();
    }
}
