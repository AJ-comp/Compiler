using ActiproSoftware.Windows.Controls.Docking;
using ApplicationLayer.ViewModels.DockingItemViewModels;
using System.Collections.Generic;

namespace ApplicationLayer.WpfApp.Converters
{
    public class DockingWindowEventConverter : EventArgsConverterExtension<DockingWindowEventConverter>
    {
        public override object Convert(object value, object parameter)
        {
            if (!(value is DockingWindowsEventArgs arg)) return null;

            List<DockingItemViewModel> result = new List<DockingItemViewModel>();
            foreach(var window in arg.Windows)
            {
                result.Add(window.DataContext as DockingItemViewModel);
            }

            return result;
        }
    }
}
