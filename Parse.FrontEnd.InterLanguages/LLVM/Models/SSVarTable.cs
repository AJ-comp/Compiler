using Parse.FrontEnd.InterLanguages.Datas;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class SSVarTable
    {
        private int _index = 0;

        private HashSet<LocalSSVar> _localVarList = new HashSet<LocalSSVar>();
        private HashSet<GlobalSSVar> _globalVarList = new HashSet<GlobalSSVar>();

        public int LocalSSVarCount => _localVarList.Count;
        public int GlobalSSVarCount => _globalVarList.Count;

        public int NextOffset => _index + 1;

        private LocalSSVar CreateNewSSVar(DataType dataType)
        {
            LocalSSVar newItem = new LocalSSVar(NextOffset, dataType);

            _localVarList.Add(newItem);
            return newItem;
        }

        public GlobalSSVar CreateNewGlobalTable(IRVarData varData)
        {
            GlobalSSVar newItem = new GlobalSSVar(varData.Type, varData);

            return newItem;
        }

        public LocalSSVar CreateToLocalTable(IRVarData varData)
        {
            LocalSSVar newItem = new LocalSSVar(NextOffset, varData.Type, varData);

            return newItem;
        }

        public LocalSSVar CreateNewSSVar(SSVarData varData)
        {
            LocalSSVar newItem = new LocalSSVar(NextOffset, varData.Type, varData);

            return newItem;
        }

        /// <summary>
        /// This function returns SSVarData whitch linked 'varData'.
        /// </summary>
        /// <param name="varData"></param>
        /// <returns></returns>
        public SSVarData Get(IRVarData varData)
        {
            SSVarData result = null;

            foreach (var ssvar in this._globalVarList)
            {
                if (ssvar.IsLinkedObject(varData) == false) continue;

                result = ssvar;
                return result;
            }

            foreach (var ssvar in this._localVarList)
            {
                if (ssvar.IsLinkedObject(varData) == false) continue;

                result = ssvar;
                return result;
            }

            return result;
        }

        /// <summary>
        /// This function returns LocalSSVar whitch linked 'ssVarData'.
        /// </summary>
        /// <param name="ssVarData"></param>
        /// <returns></returns>
        public LocalSSVar Get(SSVarData ssVarData)
        {
            LocalSSVar result = null;

            foreach (var ssvar in this._localVarList)
            {
                if (ssvar.IsLinkedObject(ssVarData) == false) continue;

                result = ssvar;
                return result;
            }

            return result;
        }

        /// <summary>
        /// This function returns a last item in the GlobalSSVarList.
        /// </summary>
        /// <returns></returns>
        public GlobalSSVar LastInGlobalList() => _globalVarList.Last();
        /// <summary>
        /// This function returns a last item in the LocalSSVarList.
        /// </summary>
        /// <returns></returns>
        public LocalSSVar LastInLocalList() => _localVarList.Last();

        public void Clear()
        {
            _localVarList.Clear();
            _globalVarList.Clear();
        }
    }
}
