using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR;
using System;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public abstract class MiniCNode : SdtsNode
    {
        public MiniCSymbolTable SymbolTable { get; protected set; }
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

        public MiniCSdtsParams CreateParamForNewBlock(SdtsParams paramToCopy)
        {
            MiniCSdtsParams result = paramToCopy.CloneForNewBlock() as MiniCSdtsParams;
            SymbolTable = result.SymbolTable;

            result.BlockLevel++;

            return result;
        }

        public override string ToString() => this.GetType().Name;


        private bool _isNotUsed = false;
    }
}
