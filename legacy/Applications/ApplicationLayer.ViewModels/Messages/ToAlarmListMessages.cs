using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd.Support.EventArgs;

namespace ApplicationLayer.ViewModels.Messages
{
    public class AlarmMessage : MessageBase
    {
        public EditorTypeViewModel Editor { get; }
        public AlarmCollection AlarmDatas { get; }

        public AlarmMessage(EditorTypeViewModel editor, AlarmCollection alarmDatas)
        {
            Editor = editor;
            AlarmDatas = alarmDatas;
        }
    }
}
