using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.WpfApp.Views.DialogViews;
using GalaSoft.MvvmLight.Messaging;
using System.Globalization;
using System.Windows;
using System.Windows.Interactivity;
using WPFLocalizeExtension.Engine;

namespace ApplicationLayer.WpfApp.Behaviors
{
    class WindowLoadBehavior : Behavior<MainWindow>
    {
        private MainWindow mainWindow;
        private QuestionToSaveDialog questionToSaveDialog = new QuestionToSaveDialog();

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            this.mainWindow = sender as MainWindow;

            this.questionToSaveDialog.Owner = mainWindow;
            this.questionToSaveDialog.ShowInTaskbar = false;

            Messenger.Default.Register<ShowSaveDialogMessage>(this, (msg) => 
            {
                var viewModel = this.questionToSaveDialog.DataContext as QuestionToSaveViewModel;
                viewModel.SaveRequest += (s, ei) => msg.ResultStatus = ShowSaveDialogMessage.Result.Yes;
                viewModel.IgnoreRequest += (s, ei) => msg.ResultStatus = ShowSaveDialogMessage.Result.No;
                viewModel.CancelRequest += (s, ei) => msg.ResultStatus = ShowSaveDialogMessage.Result.Cancel;

                this.questionToSaveDialog.ShowDialog();
            });

            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentCulture; //new CultureInfo("ko-KR");


            //            this.editor.SetComponents(this.parser);

            /*
            IHighlightingStyleRegistry registry = AmbientHighlightingStyleRegistry.Instance;
            registry.Register(ClassificationTypes.Keyword, new HighlightingStyle(Brushes.Blue));
            this.editor.HighlightingStyleRegistry = registry;

            CompletionSession session = new CompletionSession();
            session.CanHighlightMatchedText = true;
            session.Items.Add(new CompletionItem("bool", new CommonImageSourceProvider(CommonImage.Keyword)));
            session.Items.Add(new CompletionItem("int", new CommonImageSourceProvider(CommonImage.Keyword)));
            session.Items.Add(new CompletionItem("Struct", new CommonImageSourceProvider(CommonImage.Keyword)));

            this.editor.IsDelimiterHighlightingEnabled = true;
            this.editor.IsCurrentLineHighlightingEnabled = true;

            this.editor.IntelliPrompt.Sessions.Add(session);    // <-- error [NullReferenceException]
            */
        }
    }
}
