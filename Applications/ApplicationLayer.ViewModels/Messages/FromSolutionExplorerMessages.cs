using ApplicationLayer.Models.SolutionPackage;
using GalaSoft.MvvmLight.Messaging;

namespace ApplicationLayer.ViewModels.Messages
{
    public class OpenFileMessage : MessageBase
    {
        public FileStruct SelectedFile { get; }

        public OpenFileMessage(FileStruct selectedFile)
        {
            this.SelectedFile = selectedFile;
        }
    }
}
