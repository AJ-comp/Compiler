using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract partial class AJNode : SdtsNode
    {
        public AJSymbolTable SymbolTable { get; protected set; }
        public int BlockLevel { get; protected set; } = -1;

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

        public Func<SdtsNode, object, IRExpression> ConvertingToIRExpression;
        public IRExpression ExecuteToIRExpression(object param)
        {
            return ConvertingToIRExpression.Invoke(this, param);
        }

        protected AJNode(AstSymbol node)
        {
            Ast = node;
        }

        public override string ToString() => this.GetType().Name;



        private bool _isNotUsed = false;
    }
}
