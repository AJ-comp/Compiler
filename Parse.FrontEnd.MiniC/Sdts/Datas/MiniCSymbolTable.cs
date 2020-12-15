using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class MiniCSymbolTable : SymbolTable
    {
        public MiniCSymbolTable Base { get; }

        public VarTable VarTable => _varTable;

        /// <summary>
        /// For example this class has the structure as below.
        /// namespace 1 - Ref1
        ///                     - Ref2
        ///                     - Ref3
        /// namespace 2 - Ref1
        ///                     - Ref2
        ///                     - Ref3
        /// funcdef 1       - Ref1
        ///                     - Ref2
        ///                     - Ref3
        /// funcdef 2       - Ref1
        ///                     - Ref2
        ///                     - Ref3
        ///         ...
        /// </summary>
        public MiniCReferenceTable<ISymbolData> Nodes { get; } = new MiniCReferenceTable<ISymbolData>();


        public int BaseCount
        {
            get
            {
                int result = 0;

                MiniCSymbolTable baseTable = this.Base;
                while (baseTable != null)
                {
                    result++;
                    baseTable = baseTable.Base;
                }

                return result;
            }
        }

        public IEnumerable<VarTable> AllVarTable
        {
            get
            {
                List<VarTable> result = new List<VarTable>();

                MiniCSymbolTable currentTable = this;
                while (currentTable != null)
                {
                    result.Add(currentTable.VarTable);
                    currentTable = currentTable.Base;
                }

                return result;
            }
        }

        public string DebuggerDisplay
        {
            get
            {
                int funcCount = 0;
                int namespaceCount = 0;

                foreach (var item in Nodes)
                {
                    if (item is NamespaceNode) namespaceCount++;
                    else if (item is FuncDefNode) funcCount++;
                }

                return string.Format("Namespace items: {0}, Func items: {1}, VarTable items : {2}, Base count: {3}",
                                                namespaceCount,
                                                funcCount,
                                                _varTable.Count(),
                                                BaseCount);
            }
        }



        public VariableMiniC GetVarByName(string name)
        {
            VariableMiniC result = null;

            foreach (var varTable in AllVarTable)
            {
                result = varTable.GetMatchedItemWithName(name);

                if (result != null) break;
            }

            return result;
        }

        public FuncDefNode GetFuncByName(string name)
        {
            FuncDefNode result = null;

            foreach (var record in Nodes)
            {
                if (!(record.DefineField is FuncDefNode)) continue;
                if (record.DefineField.Name != name) continue;

                result = record.DefineField as FuncDefNode;
                break;
            }

            return result;
        }


        public NamespaceNode GetNamespaceByName(string name)
        {
            NamespaceNode result = null;

            foreach (var record in Nodes)
            {
                if (!(record.DefineField is NamespaceNode)) continue;
                if (record.DefineField.Name != name) continue;

                result = record.DefineField as NamespaceNode;
                break;
            }

            return result;
        }


        public MiniCSymbolTable(MiniCSymbolTable @base = null)
        {
            Base = @base;
        }

        private VarTable _varTable = new VarTable();
    }
}
