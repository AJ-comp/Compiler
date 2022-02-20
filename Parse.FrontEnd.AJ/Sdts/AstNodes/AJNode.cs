using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract partial class AJNode : SdtsNode
    {
        public ProgramNode RootNode { get; private set; }
        public int BlockLevel { get; private set; } = 0;
        public int Offset { get; private set; } = 0;

        public string FileFullPath => RootNode.FullPath;


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
            if (param != null)
            {
                RootNode = param.RootNode as ProgramNode;
                BlockLevel = param.BlockLevel;
                Offset = param.Offset;
            }

            Alarms.Clear();
            RootNode.UnLinkedSymbols.Remove(this);
            RootNode.LinkedSymbols.Remove(this);
            RootNode.AmbiguityLinkedSymbols.Remove(this);
            RootNode.CompletedSymbols.Remove(this);

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
