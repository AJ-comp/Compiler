using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Properties;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR;
using System;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public abstract partial class MiniCNode : SdtsNode
    {
        public MiniCSymbolTable SymbolTable { get; protected set; }
        public int BlockLevel { get; protected set; } = -1;

        public int ParentBlockLevel
        {
            get
            {
                int result = -1;
                var travNode = this.Parent as MiniCNode;

                while (travNode != null)
                {
                    if (travNode.BlockLevel != -1)
                    {
                        result = travNode.BlockLevel;
                        break;
                    }

                    travNode = travNode.Parent as MiniCNode;
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

        public Func<SdtsNode, object, IRExpression> ConvertingToIRExpression;
        public IRExpression ExecuteToIRExpression(object param)
        {
            return ConvertingToIRExpression.Invoke(this, param);
        }

        protected MiniCNode(AstSymbol node)
        {
            Ast = node;
        }

        public override string ToString() => this.GetType().Name;



        private bool _isNotUsed = false;
    }
}
