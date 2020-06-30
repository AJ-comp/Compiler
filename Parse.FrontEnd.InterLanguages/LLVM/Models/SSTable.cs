using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class SSTable
    {
        private int _index = 0;

        private HashSet<SSNode> _ssNodeList = new HashSet<SSNode>();
        private HashSet<GlobalVarItem> _globalVarList = new HashSet<GlobalVarItem>();

        public int LocalSSVarCount => _ssNodeList.Count;
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
        /// This function returns SSNode whitch has 'varData'.
        /// </summary>
        /// <param name="varData"></param>
        /// <returns></returns>
        public SSNode GetNode(IRVar varData)
        {
            SSNode result = null;
            GlobalVarItem foundItem = null;

            foreach (var globalVar in _globalVarList)
            {
                if (!globalVar.IsEqualWithIRVar(varData)) continue;

                foundItem = globalVar;
            }

            if (foundItem == null) return null;

            foreach (var ssNode in this._ssNodeList)
            {
                if (!ssNode.LinkedObject.Equals(foundItem)) continue;

                result = ssNode;
                return result;
            }

            return result;
        }

        /// <summary>
        /// This function returns SSNode whitch has 'localVar'.
        /// </summary>
        /// <param name="varData"></param>
        /// <returns></returns>
        public SSNode GetNode(LocalVar localVar)
        {
            SSNode result = null;

            foreach (var ssNode in this._ssNodeList)
            {
                if (ssNode.SSF != localVar) continue;

                result = ssNode;
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
        public SSNode LastInLocalList() => _ssNodeList.Last();

        public void Clear()
        {
            _ssNodeList.Clear();
            _globalVarList.Clear();
        }
    }
}
