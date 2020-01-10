using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Parse.WpfControls.AttachedProperties
{
    public class TextBlockExtension : DependencyObject
    {
        #region BoldIndexes
        public static string GetBoldIndexes(DependencyObject item)
        {
            return (string)item.GetValue(BoldIndexesProperty);

            /*
            var collection = (BoldIndexCollection)item.GetValue(BoldIndexesProperty);
            if (collection == null)
            {
                collection = new BoldIndexCollection();
                item.SetValue(BoldIndexesProperty, collection);
            }
            return collection;
            */
        }

        public static void SetBoldIndexes(DependencyObject obj, string value)
        {
            obj.SetValue(BoldIndexesProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanFocusOnLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoldIndexesProperty =
            DependencyProperty.RegisterAttached("BoldIndexes", typeof(string), typeof(TextBlockExtension), new PropertyMetadata(BoldIndexesChanged));
        #endregion

        #region BoldIndexes Handler
        private static void BoldIndexesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as TextBlock;

            string value = e.NewValue as string;
            List<string> boldIndex = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (boldIndex.Count == 0) return;

            string text = element.Text;

            element.Text = string.Empty;
            for(int i=0; i<text.Length; i++)
            {
                if (boldIndex.Contains(i.ToString()))
                    element.Inlines.Add(new Bold(new Run(text[i].ToString())));
                else
                    element.Inlines.Add(text[i].ToString());
            }
        }
        #endregion


    }
}
