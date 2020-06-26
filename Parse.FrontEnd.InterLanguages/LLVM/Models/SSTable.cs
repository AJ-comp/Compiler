using Parse.FrontEnd.InterLanguages.Datas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class SSTable
    {
        private int _index = 0;

        private HashSet<SSNode> _localVarList = new HashSet<SSNode>();
        private HashSet<GlobalVarItem> _globalVarList = new HashSet<GlobalVarItem>();

        public int LocalSSVarCount => _localVarList.Count;
        public int GlobalSSVarCount => _globalVarList.Count;

        public int NextOffset => _index + 1;

        public GlobalVarItem CreateNewGlobalTable(IRVar varData)
        {
            GlobalVarItem newItem = new GlobalVarItem(varData);

            return newItem;
        }

        public SSNode NewNode(IRVar irVar)
            => new SSNode(new LocalVar(irVar.Type, NextOffset), new GlobalVarItem(irVar));
        public SSNode NewNode(ISSVar namedItem) 
            => new SSNode(new LocalVar(namedItem.Type, NextOffset), namedItem);
        public SSNode NewNode(bool value)
            => new SSNode(new LocalVar(DataType.i1, NextOffset), new Condition(value));
        public SSNode NewNode(int value)
            => new SSNode(new LocalVar(DataType.i32, NextOffset), new Integer(value));
        public SSNode NewNode(double value)
            => new SSNode(new LocalVar(DataType.Double, NextOffset), new RealNumber(value));


        /// <summary>
        /// This function returns SSVarData whitch linked 'varData'.
        /// </summary>
        /// <param name="varData"></param>
        /// <returns></returns>
        public ISSVar Get(IRVar varData)
        {
            ISSVar result = null;

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
        /// This function returns LocalSSVar whitch linked with 'ssVarData'.
        /// </summary>
        /// <param name="ssVarData"></param>
        /// <returns></returns>
        public LocalVar Get(ISSVar ssVarData)
        {
            LocalVar result = null;

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
        public GlobalVarItem LastInGlobalList() => _globalVarList.Last();
        /// <summary>
        /// This function returns a last item in the LocalSSVarList.
        /// </summary>
        /// <returns></returns>
        public SSNode LastInLocalList() => _localVarList.Last();

        public void Clear()
        {
            _localVarList.Clear();
            _globalVarList.Clear();
        }
    }
}
