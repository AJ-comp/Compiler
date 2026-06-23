using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationLayer.WpfApp
{
    public class CellTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement frameworkElement = container as FrameworkElement; // System.Windows.Controls.ContentPresenter

            string resourceKey = "EmptyCellTemplate";
            if (item.GetType() == typeof(VarTreeNodeModel)) resourceKey = "VarCellTemplate";
            else if (item.GetType() == typeof(FuncTreeNodeModel)) resourceKey = "FuncCellTemplate";

            return (DataTemplate)frameworkElement.FindResource(resourceKey);
        }
    }
}
