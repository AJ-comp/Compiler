using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.Datas;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial class ClassDefNode : ISymbolCenter
    {
        public Access AccessType { get; set; } = Access.Private;
        public override AJDataType Type => AJDataType.Class;

        public override uint Size
        {
            get
            {
                uint size = 0;
                foreach (var member in Fields)
                    size += member.Type.Size;

                return size;
            }
        }
        public List<VariableAJ> Fields { get; set; } = new List<VariableAJ>();
        public List<FuncDefNode> AllFuncs { get; set; } = new List<FuncDefNode>();
        public List<AJNode> References { get; set; } = new List<AJNode>();

        public NamespaceNode Namespace
        {
            get
            {
                var tracker = Parent;

                while (true)
                {
                    if (tracker is null) return null;
                    if (tracker is NamespaceNode) return tracker as NamespaceNode;

                    tracker = tracker.Parent;
                }
            }
        }


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
                result.AddRange(Fields);
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
            => AllFuncs.Where((func) => func.Type == FuncType.Normal);


        /*****************************************/
        /// <summary>
        /// <i>interface property</i>   <br/>
        /// Creator List
        /// </summary>
        /*****************************************/
        public IEnumerable<FuncDefNode> CreatorList
            => AllFuncs.Where((func) => func.Type == FuncType.Creator);


        /*****************************************/
        /// <summary>
        /// <i>interface property</i>   <br/>
        /// Destructor
        /// </summary>
        /*****************************************/
        public FuncDefNode Destructor
            => AllFuncs.Where(func => func.Type == FuncType.Destructor).FirstOrDefault();



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
            => $"{AccessType} class {Name} (field: {Fields.Count()}, func: {AllFuncs.Count()}) [{GetType()}]";


        protected override bool IsDuplicated(TokenData tokenToAdd)
        {
            foreach (var field in Fields)
            {
                if (field.Name != tokenToAdd.Input) continue;

                AddDuplicatedErrorInType(tokenToAdd);
                return true;
            }

            foreach (var func in AllFuncs)
            {
                if (func.Name != tokenToAdd.Input) continue;

                AddDuplicatedErrorInType(tokenToAdd);
                return true;
            }

            return false;
        }
    }
}
