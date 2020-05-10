using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.ViewModels.Messages
{
    public class TreeSymbolMessage : MessageBase
    {
        public TreeSymbol TreeSymbol { get; }

        public TreeSymbolMessage(TreeSymbol treeSymbol)
        {
            TreeSymbol = treeSymbol;
        }
    }
}
