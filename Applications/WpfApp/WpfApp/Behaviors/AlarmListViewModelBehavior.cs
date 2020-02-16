using ApplicationLayer.ViewModels.ToolWindowViewModels;
using ApplicationLayer.WpfApp.Views.WindowViews;
using System.Windows.Interactivity;

namespace ApplicationLayer.WpfApp.Behaviors
{
    class AlarmListViewModelBehavior : Behavior<AlarmList>
    {


        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseDoubleClick -= AssociatedObject_MouseDoubleClick;

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
        }

        private void AssociatedObject_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AlarmListViewModel viewModel = this.AssociatedObject.DataContext as AlarmListViewModel;

//          this.AssociatedObject.alarmList.SelectedIndex
        }
    }
}
