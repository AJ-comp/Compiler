using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.Datas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial class ClassDefNode : ISymbolData
    {
        public Access AccessType { get; set; } = Access.Private;
        public TokenData NameToken { get; set; }
        public override AJDataType Type => AJDataType.Class;
        public int Block { get; set; }
        public int Offset { get; set; }
        public override uint Size => AJUtilities.SizeOf(this);
        public override string Name => NameToken.Input;
        public List<VariableAJ> Fields { get; set; } = new List<VariableAJ>();
        public List<FuncDefNode> AllFuncs { get; set; } = new List<FuncDefNode>();
        public List<AJNode> References { get; set; } = new List<AJNode>();


        public override string FullName
        {
            get
            {
                string result = string.Empty;

                if (Parent is NamespaceNode)
                {
                    var parent = Parent as NamespaceNode;
                    result = parent.FullName;
                }
                else if (Parent is ClassDefNode)
                {
                    var parent = Parent as ClassDefNode;
                    result = parent.FullName;
                }

                return $"{result}.{Name}";
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

        public override bool Equals(object obj)
        {
            return obj is ClassDefNode node &&
                   Name == node.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(ClassDefNode left, ClassDefNode right)
        {
            return EqualityComparer<ClassDefNode>.Default.Equals(left, right);
        }

        public static bool operator !=(ClassDefNode left, ClassDefNode right)
        {
            return !(left == right);
        }

        private string GetDebuggerDisplay()
            => $"{AccessType} class {Name} (field: {Fields.Count()}, func: {AllFuncs.Count()})";


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
