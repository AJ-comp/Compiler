﻿using ActiproSoftware.Products.Docking;
using ActiproSoftware.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.WpfApp
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public ResourceDictionary ThemeDictionary
        {
            // You could probably get it via its name with some query logic as well.
            get { return Resources.MergedDictionaries[2]; }
        }

        public void ChangeTheme(Uri uri)
        {
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        }

        public void ChangeTheme(List<Uri> uris)
        {
            foreach (var uri in uris)
                ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });

            var dictionary = ThemeDictionary.MergedDictionaries.ToList();

            foreach(var item in dictionary)
            {
                if (uris.Contains(item.Source)) continue;
                ThemeDictionary.MergedDictionaries.Remove(item);
            }
        }

        private void ActiProControlLocalize()
        {
            SR.SetCustomString(SRName.UIToolWindowContainerCloseButtonToolTip.ToString(), CommonResource.Close);
            SR.SetCustomString(SRName.UIToolWindowContainerToggleAutoHideButtonToolTip.ToString(), CommonResource.AutoHide);
            SR.SetCustomString(SRName.UIToolWindowContainerOptionsButtonToolTip.ToString(), CommonResource.WindowPosition);

            SR.SetCustomString(SRName.UICommandMakeFloatingWindowText.ToString(), CommonResource.Float);
            SR.SetCustomString(SRName.UICommandMakeDockedWindowText.ToString(), CommonResource.Dock);
            SR.SetCustomString(SRName.UICommandMakeDocumentWindowText.ToString(), CommonResource.DockAsDocument);
            SR.SetCustomString(SRName.UICommandToggleWindowAutoHideStateText.ToString(), CommonResource.AutoHide);
            SR.SetCustomString(SRName.UICommandCloseWindowText.ToString(), CommonResource.Close);
            SR.SetCustomString(SRName.UICommandCloseOthersText.ToString(), CommonResource.AllCloseExceptThisWindow);
            SR.SetCustomString(SRName.UICommandCloseAllInContainerText.ToString(), CommonResource.CloseTabGroup);
            SR.SetCustomString(SRName.UICommandCloseAllDocumentsText.ToString(), CommonResource.CloseAll);
            SR.SetCustomString(SRName.UICommandPinTabText.ToString(), CommonResource.PinTab);
            SR.SetCustomString(SRName.UICommandMoveToNewHorizontalContainerText.ToString(), CommonResource.NewHorTabGroup);
            SR.SetCustomString(SRName.UICommandMoveToNewVerticalContainerText.ToString(), CommonResource.NewVerTabGroup);
            SR.SetCustomString(SRName.UICommandMoveToPreviousContainerText.ToString(), CommonResource.PreviousTabGroup);
            //SR.SetCustomString(SRName.UICommandMoveToPrimaryMdiHostText.ToString(), CommonResource.PinTab);

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.BeginUpdate();
            try
            {
                this.ActiProControlLocalize();

                // The Office 2010 and Luna themes are in separate assemblies and must be registered if you will use them in the application
                // ThemesOfficeThemeCatalogRegistrar.Register();
                // ThemesLunaThemeCatalogRegistrar.Register();

                // Use the Actipro styles for native WPF controls that look great with Actipro's control products
                ThemeManager.AreNativeThemesEnabled = true;

                // Always force a Metro accented green theme (similar to Excel)
                ThemeManager.CurrentTheme = "MetroDark";
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            }
            finally
            {
                ThemeManager.EndUpdate();
            }


            base.OnStartup(e);
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            string errorMessage = string.Format("An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError: {0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)",

            e.Exception.Message + (e.Exception.InnerException != null ? "\n" +
            e.Exception.InnerException.Message : null));

            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
