using ApplicationLayer.Common.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class HierarchicalData : INotifyPropertyChanged, IEquatable<HierarchicalData>
    {
        [XmlIgnore]
        public string CurOPath { get; set; } = string.Empty;
        [XmlIgnore]
        public string FullName { get; set; } = string.Empty;
        [XmlIgnore]
        public string ImageSource { get; set; }
        [XmlIgnore]
        public HierarchicalData Parent { get; internal set; } = null;
        [XmlIgnore]
        public string NameWithoutExtension => Path.GetFileNameWithoutExtension(this.FullName);
        [XmlIgnore]
        public string FullPath => Path.Combine(this.BaseOPath, this.FullName);
        [XmlIgnore]
        public string RelativePath => Path.Combine(this.CurOPath, this.FullName);
        [XmlIgnore]
        public string AutoPath => (this.IsAbsolutePath) ? this.FullPath : this.RelativePath;
        [XmlIgnore]
        public bool IsRoot => (this.Parent == null);
        [XmlIgnore]
        public bool IsInRootPath => PathHelper.ComparePath(this.Root.BaseOPath, this.BaseOPath);
        [XmlIgnore]
        public bool IsAbsolutePath
        {
            get
            {
                if (this.CurOPath.Length == 0) return false;
                // If CurOPath doesn't include drive path then is the relatve path.
                if(PathHelper.IsDrivePath(this.CurOPath) == false) return false;

                return (PathHelper.ComparePath(this.Root.BaseOPath, this.CurOPath) == false);
            }
        }

        [XmlIgnore]
        public HierarchicalData Root
        {
            get
            {
                HierarchicalData current = this;
                while (current.Parent != null) current = current.Parent;

                return current;
            }
        }


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
        public string BaseOPath
        {
            get
            {
                string result = this.CurOPath;

                if (PathHelper.IsDrivePath(result)) return result;

                HierarchicalData current = this;
                while (current.Parent != null)
                {
                    result = Path.Combine(current.Parent.CurOPath, result);
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

        public bool Equals(HierarchicalData other) => this.FullPath == other.FullPath;
    }
}
