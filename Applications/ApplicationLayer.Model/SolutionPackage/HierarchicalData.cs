using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Common.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class HierarchicalData : INotifyPropertyChanged, ISaveable
    {
        [XmlIgnore]
        public string CurOPath { get; set; } = string.Empty;
        private string fullName = string.Empty;
        [XmlIgnore]
        public string FullName
        {
            get => this.fullName;
            set
            {
                this.fullName = value;
                this.OnPropertyChanged(nameof(FullName));
            }
        }
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
                this.IsEditMode = false;
                this.OnPropertyChanged(nameof(IsSelected));
            }
        }


        private bool isEditMode;
        [XmlIgnore]
        public bool IsEditMode
        {
            get => this.isEditMode;
            set
            {
                this.isEditMode = value;
                this.OnPropertyChanged(nameof(IsEditMode));
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

        public abstract string DisplayName { get; }
        private string toChangeDisplayName = string.Empty;
        [XmlIgnore]
        public string ToChangeDisplayName
        {
            get => this.toChangeDisplayName;
            set
            {
                this.toChangeDisplayName = value;
                this.OnPropertyChanged(nameof(ToChangeDisplayName));
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

        public HierarchicalData(string curOpath, string fullName)
        {
            CurOPath = curOpath;
            FullName = fullName;
        }

        public abstract void Save();
        public abstract void ChangeDisplayName();
        public abstract void CancelChangeDisplayName();
        public virtual ExceptionData IsValidToChange()
        {
            ExceptionData result = null;

            if (this.ToChangeDisplayName == string.Empty)
                result = new ExceptionData(ExceptionKind.Error, CommonResource.MustInputName);
            else if (Regex.IsMatch("[A-Za-z0-9_-]*\\.*[A-Za-z0-9]{3,4}", this.toChangeDisplayName))
                result = new ExceptionData(ExceptionKind.Error, CommonResource.MustInputName);

            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is HierarchicalData data &&
                   FullPath == data.FullPath;
        }

        public override int GetHashCode()
        {
            return 2018552787 + EqualityComparer<string>.Default.GetHashCode(FullPath);
        }
    }
}
