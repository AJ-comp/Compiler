using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract partial class AJNode : SdtsNode, IData, IHasParent
    {
        public int Id { get; set; } = _nextId++;
        public int ParentId { get; set; }
        public string ParentType { get; set; }
        public int ChildIndex { get; set; }
        public int BlockLevel => CompileData.BlockLevel;


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

        public override SdtsNode Compile(CompileParameter param)
        {
            if (param != null) CompileData = param;

            return this;
        }

        public override string ToString() => this.GetType().Name;

        public override bool Equals(object obj)
        {
            return obj is AJNode node &&
                   Id == node.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        private bool _isNotUsed = false;
        private static int _nextId = 0;

        public static bool operator ==(AJNode left, AJNode right)
        {
            return EqualityComparer<AJNode>.Default.Equals(left, right);
        }

        public static bool operator !=(AJNode left, AJNode right)
        {
            return !(left == right);
        }
    }
}
