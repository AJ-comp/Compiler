using GalaSoft.MvvmLight.Messaging;

namespace ApplicationLayer.ViewModels.Messages
{
    public class AddBuildMessage : MessageBase
    {
        public string Message { get; }

        public AddBuildMessage(string message)
        {
            Message = message;
        }
    }
}
