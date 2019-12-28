using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Utilities
{
    public class ViewCache : UserControl
    {
        public ViewCache()
        {
            this.Unloaded += this.ViewCache_Unloaded;
        }

        void ViewCache_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= this.ViewCache_Unloaded;
            this.Content = null;
        }

        private Type _contentType;
        public Type ContentType
        {
            get { return this._contentType; }
            set
            {
                if (this._contentType == value) return;

                this._contentType = value;
                this.Content = ViewFactory.GetView(this._contentType);
            }
        }
    }
}
