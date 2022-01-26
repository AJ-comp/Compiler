using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract partial class AJNode : SdtsNode, IData, IHasParent
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string ParentType { get; set; }
        public int ChildIndex { get; set; }
        public int BlockLevel { get; protected set; } = -1;

        public CompileParameter NodeInfos { get; set; }


        public int ParentBlockLevel
        {
            get
            {
                int result = -1;
                var travNode = this.Parent as AJNode;

                while (travNode != null)
                {
                    if (travNode.BlockLevel != -1)
                    {
                        result = travNode.BlockLevel;
                        break;
                    }

                    travNode = travNode.Parent as AJNode;
                }

                return result;
            }
        }

        public bool IsNotUsed
        {
            get => _isNotUsed;
            set
            {
                _isNotUsed = value;

                foreach (var token in AllTokens)
                    token.IsNotUsed = value;
            }
        }

        protected AJNode(AstSymbol node)
        {
            Ast = node;
        }

        public override string ToString() => this.GetType().Name;



        private bool _isNotUsed = false;
    }
}
