using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.ViewModels.Messages
{
    public class AddEditorMessage : MessageBase
    {
        public EditorTypeViewModel AddEditor { get; }

        public AddEditorMessage(EditorTypeViewModel addEditor)
        {
            AddEditor = addEditor;
        }
    }
}
