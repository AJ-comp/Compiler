using ActiproSoftware.Windows.Controls;
using ApplicationLayer.Models.ToolWindowStatus;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters
{
    class ToolItemDockSideConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dockSide = (ToolItemDockSide)value;
            switch (dockSide)
            {
                case ToolItemDockSide.Left:
                    return Side.Left;
                case ToolItemDockSide.Top:
                    return Side.Top;
                case ToolItemDockSide.Right:
                    return Side.Right;
                default:
                    return Side.Bottom;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dockSide = (Side)value;
            switch (dockSide)
            {
                case Side.Left:
                    return ToolItemDockSide.Left;
                case Side.Top:
                    return ToolItemDockSide.Top;
                case Side.Right:
                    return ToolItemDockSide.Right;
                default:
                    return ToolItemDockSide.Bottom;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
