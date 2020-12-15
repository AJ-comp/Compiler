using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public class MiniCReferenceTable<T> : IEnumerable<MiniCReferenceRecord<T>> where T : ISymbolData
    {
        public bool AddReferenceRecord(T baseForm, ReferenceInfo referenceInfo)
        {
            bool result = false;

            foreach (var block in _recordBlocks)
            {
                if (block.DefineField.Equals(baseForm))
                {
                    result = true;
                    block.ReferenceList.Add(referenceInfo);
                    break;
                }
            }

            return result;
        }

        public virtual bool CreateNewBlock(T baseForm, MiniCNode miniCNode) => CreateNewBlock(baseForm, new ReferenceInfo(miniCNode));

        public virtual bool CreateNewBlock(T baseForm, ReferenceInfo referenceInfo)
        {
            var recordToAdd = new MiniCReferenceRecord<T>(baseForm, referenceInfo);
            if (_recordBlocks.Contains(recordToAdd)) return false;

            _recordBlocks.Add(recordToAdd);
            return true;
        }

        public T GetMatchedItemWithName(string name)
        {
            T result = default;

            foreach (var block in _recordBlocks)
            {
                if (block.DefineField.Name == name)
                {
                    result = block.DefineField;
                    break;
                }
            }

            return result;
        }

        public IEnumerator<MiniCReferenceRecord<T>> GetEnumerator() => (_recordBlocks).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (_recordBlocks).GetEnumerator();


        private HashSet<MiniCReferenceRecord<T>> _recordBlocks = new HashSet<MiniCReferenceRecord<T>>();
    }


    /// <summary>
    /// This class has the structure as below.
    /// Var1 - Ref1
    ///         - Ref2
    ///         - Ref3
    ///         ...
    ///         
    /// Var2 - Ref1
    ///         - Ref2
    ///         - Ref3
    /// </summary>
    public class VarTable : MiniCReferenceTable<VariableMiniC>
    {

    }
}
