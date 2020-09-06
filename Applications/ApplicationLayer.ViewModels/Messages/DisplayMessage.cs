using ApplicationLayer.Models;
using GalaSoft.MvvmLight.Messaging;

namespace ApplicationLayer.ViewModels.Messages
{
    public class DisplayMessage : MessageBase
    {
        public MessageData Data { get; }
        public string Title { get; }

        public DisplayMessage(MessageData data, string title)
        {
            Data = data;
            Title = title;
        }

        public DisplayMessage(string message, string title)
        {
            Data = new MessageData(MessageKind.Information, message);
            Title = title;
        }
    }
}
