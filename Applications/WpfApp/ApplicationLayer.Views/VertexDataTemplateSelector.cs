using ApplicationLayer.Models.GraphModels;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationLayer.Views
{
    class VertexDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement frameworkElement = container as FrameworkElement; // System.Windows.Controls.ContentPresenter

            string resourceKey = string.Empty;
            if (item.GetType() == typeof(EbnfTreeVertex)) resourceKey = "ebnfTreeVertexTemplate";
            else if (item.GetType() == typeof(TreeSymbolVertex)) resourceKey = "treeSymbolVertexTemplate";

            return (DataTemplate)frameworkElement.FindResource(resourceKey);
        }
    }
}
