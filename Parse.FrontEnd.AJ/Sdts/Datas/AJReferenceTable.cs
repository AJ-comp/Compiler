using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public class AJReferenceTable<T> : IEnumerable<AJReferenceRecord<T>> where T : ISymbolData
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

        public virtual bool CreateNewBlock(T baseForm, AJNode miniCNode) => CreateNewBlock(baseForm, new ReferenceInfo(miniCNode));

        public virtual bool CreateNewBlock(T baseForm, ReferenceInfo referenceInfo)
        {
            var recordToAdd = new AJReferenceRecord<T>(baseForm, referenceInfo);
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

        public IEnumerator<AJReferenceRecord<T>> GetEnumerator() => (_recordBlocks).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (_recordBlocks).GetEnumerator();


        private HashSet<AJReferenceRecord<T>> _recordBlocks = new HashSet<AJReferenceRecord<T>>();
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
    public class VarTable : AJReferenceTable<VariableMiniC>
    {

    }
}
