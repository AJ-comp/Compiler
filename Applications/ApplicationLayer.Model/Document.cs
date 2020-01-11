namespace ApplicationLayer.Models
{
    public class Document
    {
        public string ImageSource { get; }
        public string ItemName { get; }
        public string Explain { get; }

        public Document(string imageSource, string itemName, string explain)
        {
            ImageSource = imageSource;
            ItemName = itemName;
            Explain = explain;
        }
    }
}
