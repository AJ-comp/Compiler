using Parse.FrontEnd.InterLanguages.Datas.Types;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class SSTable
    {
        private int _index = 0;

        private HashSet<SSNode> _ssNodeList = new HashSet<SSNode>();
        private HashSet<GlobalVar> _globalVarList = new HashSet<GlobalVar>();

        public int LocalSSVarCount => _ssNodeList.Count;
        public int GlobalSSVarCount => _globalVarList.Count;

        public int NextOffset
        {
            get => _index + 1;
            private set => NextOffset = value;
        }

        public GlobalVar CreateNewGlobalVar(IRVar irVar)
        {
            Type myType = typeof(GlobalVar<>).MakeGenericType(irVar.Type.GetType());
            GlobalVar globalVar  = Activator.CreateInstance(myType, irVar) as GlobalVar;

            _globalVarList.Add(globalVar);
            return globalVar;
        }

        public SSNode NewNode(IRVar irVar)
        {
            // ssf
            Type ssfType = typeof(LocalVar<>).MakeGenericType(irVar.Type.GetType());
            object newSSVar = Activator.CreateInstance(ssfType, NextOffset++);

            var ssNode = new SSNode(newSSVar as LocalVar, null);
            _ssNodeList.Add(ssNode);

            return ssNode;
        }

        public SSNode NewNode(ISSVar namedItem)
        {
            // ssf
            Type myType = typeof(LocalVar<>).MakeGenericType(namedItem.Type.GetType());
            object instance = Activator.CreateInstance(myType, NextOffset++);

            var ssNode = new SSNode(instance as LocalVar, namedItem);
            _ssNodeList.Add(ssNode);

            return ssNode;
        }

        public SSNode NewNode(bool value)
        {
            var ssNode = new SSNode(new LocalVar<Bit>(NextOffset++), new SSValue<Bit>(value));
            _ssNodeList.Add(ssNode);

            return ssNode;
        }

        /// <summary>
        /// This function returns SSNode whitch has 'varData'.
        /// </summary>
        /// <param name="varData"></param>
        /// <returns></returns>
        public SSNode GetNode(IRVar varData)
        {
            SSNode result = null;
            GlobalVar foundItem = null;

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
        public GlobalVar LastInGlobalList() => _globalVarList.Last();
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
