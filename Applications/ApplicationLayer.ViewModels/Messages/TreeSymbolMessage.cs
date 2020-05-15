using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd.Ast;

namespace ApplicationLayer.ViewModels.Messages
{
    public class TreeSymbolMessage : MessageBase
    {
        public AstSymbol TreeSymbol { get; }

        public TreeSymbolMessage(AstSymbol treeSymbol)
        {
            TreeSymbol = treeSymbol;
        }
    }
}
