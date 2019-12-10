namespace WpfApp.Models
{
    public class NewFileData
    {
        public string ImageSource { get; }
        public string ItemName { get; }
        public string Explain { get; }

        public NewFileData(string imageSource, string itemName, string explain)
        {
            ImageSource = imageSource;
            ItemName = itemName;
            Explain = explain;
        }
    }
}
