using ApplicationLayer.Models;
using System;
using System.Windows;

namespace ApplicationLayer.WpfApp
{
    class MessageBoxLogic : IMessageBoxService
    {
        public void ShowError(Exception Error, string Title) => MessageBox.Show(Error.ToString(), Title, MessageBoxButton.OK, MessageBoxImage.Error);
        public void ShowError(string Message, string Title) => MessageBox.Show(Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        public void ShowInfo(string Message, string Title) => MessageBox.Show(Message, Title, MessageBoxButton.OK, MessageBoxImage.Information);
        public void ShowMessage(string Message, string Title) => MessageBox.Show(Message, Title, MessageBoxButton.OK);
        public bool ShowQuestion(string Message, string Title) => MessageBox.Show(Message, Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        public void ShowWarning(string Message, string Title) => MessageBox.Show(Message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
