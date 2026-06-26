using GalaSoft.MvvmLight.Messaging;
using Janglim.FrontEnd;

namespace ApplicationLayer.ViewModels.Messages
{
    public class TreeSymbolMessage : MessageBase
    {
        public SdtsNode TreeSymbol { get; }

        public TreeSymbolMessage(SdtsNode treeSymbol)
        {
            TreeSymbol = treeSymbol;
        }
    }
}
