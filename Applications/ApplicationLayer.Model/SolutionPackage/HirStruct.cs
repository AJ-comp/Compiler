using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class HirStruct : INotifyPropertyChanged
    {
        [XmlIgnore]
        public string OPath { get; set; } = string.Empty;
        [XmlIgnore]
        public string FullName { get; set; } = string.Empty;
        [XmlIgnore]
        public string ImageSource { get; set; }
        [XmlIgnore]
        public HirStruct Parent { get; internal set; } = null;
        [XmlIgnore]
        public string NameWithoutExtension => Path.GetFileNameWithoutExtension(this.FullName);
        [XmlIgnore]
        public string FullPath => Path.Combine(this.BasePath, this.FullName);
        [XmlIgnore]
        public string RelativePath => Path.Combine(this.OPath, this.FullName);

        [XmlIgnore]
        public bool IsAbsolutePath { get; set; } = false;

        private bool isSelected;
        [XmlIgnore]
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                this.isSelected = value;
                this.OnPropertyChanged("IsSelected");
            }
        }

        [XmlIgnore]
        public string BasePath
        {
            get
            {
                string result = this.OPath;

                if (this.IsAbsolutePath) return result;

                HirStruct current = this;
                while (current.Parent != null)
                {
                    result = Path.Combine(current.Parent.OPath, result);
                    current = current.Parent;
                }

                return result;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
