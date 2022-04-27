using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.Datas;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial class ClassDefNode : TypeDefNode, ISymbolCenter
    {
        public override AJDataType DefType => AJDataType.Class;

        public override uint Size
        {
            get
            {
                uint size = 0;
                foreach (var member in _fields)
                    size += member.Type.Size;

                return size;
            }
        }

        public override IEnumerable<VariableAJ> AllFields => _fields;


        public IEnumerable<TokenData> FullNameTokens
        {
            get
            {
                List<TokenData> result = new List<TokenData>();

                if (Parent is NamespaceNode)
                {
                    var parent = Parent as NamespaceNode;
                    result.AddRange(parent.NameTokens);
                }
                else if (Parent is ClassDefNode)
                {
                    var parent = Parent as ClassDefNode;
                    result.AddRange(parent.FullNameTokens);
                }

                result.Add(NameToken);

                return result;
            }
        }


        public IEnumerable<ISymbolData> SymbolList
        {
            get
            {
                List<ISymbolData> result = new List<ISymbolData>();
                result.AddRange(_fields);
                result.AddRange(AllFuncs);

                return result;
            }
        }


        /*****************************************/
        /// <summary>
        /// <i>interface property</i>   <br/>
        /// Function list
        /// </summary>
        /*****************************************/
        public IEnumerable<FuncDefNode> FuncList
            => AllFuncs.Where((func) => func.FunctionalType == FuncType.Normal);


        /*****************************************/
        /// <summary>
        /// <i>interface property</i>   <br/>
        /// Creator List
        /// </summary>
        /*****************************************/
        public IEnumerable<FuncDefNode> CreatorList
            => AllFuncs.Where((func) => func.FunctionalType == FuncType.Creator);


        /*****************************************/
        /// <summary>
        /// <i>interface property</i>   <br/>
        /// Destructor
        /// </summary>
        /*****************************************/
        public FuncDefNode Destructor
            => AllFuncs.Where(func => func.FunctionalType == FuncType.Destructor).FirstOrDefault();



        /*
        public FuncDefNode GetSymbol(TokenDataList tokens, IEnumerable<VariableAJ> paramTypes)
        {
            foreach (var token in tokens)
            {
                GetSymbol(token);
            }
        }
        */

        private string GetDebuggerDisplay()
            => $"{AccessType} class {Name} (field: {_fields.Count()}, func: {AllFuncs.Count()}) [{GetType().Name}]";
    }
}
