namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class TextViewModel : DocumentViewModel
    {
        public string Source { get; private set; }

        public TextViewModel(string source, string title) : base(title)
        {
            Source = source;
        }
    }
}
