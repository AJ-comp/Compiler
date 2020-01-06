using System.Collections.ObjectModel;

namespace WpfApp.Models.MicroControllerModels
{
    public abstract class MicroController
    {
        public string Name { get; protected set; } = string.Empty;

        public string Explain { get; protected set; } = string.Empty;
        public string DataSheetPath { get; protected set; } = string.Empty;

        public override string ToString() => string.Format("Name={0}, Explain={1}, DataSheet Path={2}", this.Name, this.Explain, this.DataSheetPath);
    }
}
