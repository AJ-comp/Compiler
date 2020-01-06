using Parse.BackEnd.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfApp.Selectors
{
    public class Selector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            string templateKey;
            if (item is CollectionViewGroup)
            {
                if ((item as CollectionViewGroup).Name == null)
                {
                    return null;
                }
                templateKey = "GroupTemplate";
            }
            else if (item is ARM) templateKey = "pTemplate";
            else
                return null;
            return (DataTemplate)((FrameworkElement)container).FindResource(templateKey);
        }
    }
}
