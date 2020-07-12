using ActiproSoftware.Windows.Controls.Docking;
using ApplicationLayer.Models.ToolWindowStatus;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.WpfApp.Converters
{
    class ToolItemStateConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (ToolItemState)value;
            switch (state)
            {
                case ToolItemState.AutoHide:
                    return DockingWindowState.AutoHide;
                case ToolItemState.Docked:
                    return DockingWindowState.Docked;
                default:
                    return DockingWindowState.Document;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (DockingWindowState)value;
            switch (state)
            {
                case DockingWindowState.AutoHide:
                    return ToolItemState.AutoHide;
                case DockingWindowState.Docked:
                    return ToolItemState.Docked;
                default:
                    return ToolItemState.Document;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
