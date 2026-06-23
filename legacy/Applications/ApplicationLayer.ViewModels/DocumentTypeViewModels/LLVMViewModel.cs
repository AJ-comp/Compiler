using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class LLVMViewModel : DocumentViewModel
    {
        public string TextContent
        {
            get => _textContext;
            set
            {
                _textContext = value;
                RaisePropertyChanged(nameof(TextContent));
            }
        }


        private string _textContext;

        public LLVMViewModel(string title) : base(title)
        {
        }

        public LLVMViewModel(string title, string toolstrip) : base(title, toolstrip)
        {
        }

        public LLVMViewModel(string title, string toolstrip, string serializationId) : base(title, toolstrip, serializationId)
        {
        }
    }
}
