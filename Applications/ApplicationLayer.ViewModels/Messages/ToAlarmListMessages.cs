using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using GalaSoft.MvvmLight.Messaging;

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

    public class AlarmMessage : MessageBase
    {
        public EditorTypeViewModel Editor { get; }

        public AlarmMessage(EditorTypeViewModel editor)
        {
            Editor = editor;
        }
    }
}
