using ApplicationLayer.Models;
using ApplicationLayer.ViewModels.Messages;
using System;
using System.Windows;

namespace ApplicationLayer.WpfApp
{
    public class MessageBoxLogic
    {
        //        public bool ShowQuestion(string Message, string Title) => MessageBox.Show(Message, Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        private static MessageBoxLogic instance;
        public static MessageBoxLogic Instance
        {
            get
            {
                if (MessageBoxLogic.instance == null) instance = new MessageBoxLogic();

                return instance;
            }
        }

        public void ShowMessage(DisplayMessage message)
        {
            if (message is null) return;

            if (message.Data.Kind == MessageKind.Error)
                MessageBox.Show(message.Data.Message, message.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            else if(message.Data.Kind == MessageKind.Warning)
                MessageBox.Show(message.Data.Message, message.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            else if(message.Data.Kind == MessageKind.Information)
                MessageBox.Show(message.Data.Message, message.Title, MessageBoxButton.OK, MessageBoxImage.Information);
//            else if(message.Data.Kind == MessageKind.Question)


        }
    }
}
