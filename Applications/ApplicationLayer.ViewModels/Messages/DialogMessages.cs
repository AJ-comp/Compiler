using GalaSoft.MvvmLight.Messaging;

namespace ApplicationLayer.ViewModels.Messages
{
    public class ShowSaveDialogMessage : MessageBase
    {
        public enum Result { Yes, No, Cancel }

        public Result ResultStatus { get; set; } = Result.No;
    }

    public class HideSaveDialogMessage : MessageBase
    {
    }
}
