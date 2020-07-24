using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public abstract class MiniCNode : SdtsNode
    {
        public MiniCSymbolTable SymbolTable { get; protected set; }

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
    }
}
