/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:WpfApp"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using WpfApp.ViewModels.WindowViewModels;

namespace ApplicationLayer.WpfApp.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<GrammarInfoViewModel>();
            SimpleIoc.Default.Register<SolutionExplorerViewModel>();
        }

        public MainViewModel Main { get => ServiceLocator.Current.GetInstance<MainViewModel>(); }
        public SolutionExplorerViewModel SolutionExplorerWindow { get => ServiceLocator.Current.GetInstance<SolutionExplorerViewModel>(); }

        private void NotifyUserMethod(NotificationMessage message)
        {
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}