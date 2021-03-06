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
using ApplicationLayer.ViewModels.ToolWindowViewModels;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

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
            SimpleIoc.Default.Register<QuestionToSaveViewModel>();

            /// Related to ToolWindow
            SimpleIoc.Default.Register<SolutionExplorerViewModel>();
            SimpleIoc.Default.Register<AlarmListViewModel>();
            SimpleIoc.Default.Register<OutputViewModel>();
            SimpleIoc.Default.Register<CallStackViewModel>();
            SimpleIoc.Default.Register<TreeSymbolDetailViewModel>();
            SimpleIoc.Default.Register<TestToolWindowViewModel>();
        }

        public MainViewModel Main { get => ServiceLocator.Current.GetInstance<MainViewModel>(); }
        public QuestionToSaveViewModel QuestionToSave { get => ServiceLocator.Current.GetInstance<QuestionToSaveViewModel>(); }


        public SolutionExplorerViewModel SolutionExplorerWindow { get => ServiceLocator.Current.GetInstance<SolutionExplorerViewModel>(); }
        public AlarmListViewModel AlarmListWindow { get => ServiceLocator.Current.GetInstance<AlarmListViewModel>(); }
        public OutputViewModel OutputWindow { get => ServiceLocator.Current.GetInstance<OutputViewModel>(); }
        public CallStackViewModel CallStackWindow { get => ServiceLocator.Current.GetInstance<CallStackViewModel>(); }
        public TreeSymbolDetailViewModel TreeSymbolDetailView { get => ServiceLocator.Current.GetInstance<TreeSymbolDetailViewModel>(); }
        public TestToolWindowViewModel TestToolWindow { get => ServiceLocator.Current.GetInstance<TestToolWindowViewModel>(); }


        private void NotifyUserMethod(NotificationMessage message)
        {
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}