using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ApplicationLayer.Models
{
    public enum ThemeKind { Dark, Blue };

    public class Theme : INotifyPropertyChanged
    {
        private ThemeKind themeKind;
        public ThemeKind ThemeKind
        {
            get => this.themeKind;
            set
            {
                if (this.themeKind == value) return;

                this.themeKind = value;
                NotifyPropertyChanged();
            }
        }

        private static Theme instance;
        public static Theme Instance
        {
            get
            {
                if (Theme.instance == null) Theme.instance = new Theme();

                return Theme.instance;
            }
        }

        private Theme()
        {
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
