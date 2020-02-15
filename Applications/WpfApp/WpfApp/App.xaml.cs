using ActiproSoftware.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.BeginUpdate();
            try
            {
                // The Office 2010 and Luna themes are in separate assemblies and must be registered if you will use them in the application
                // ThemesOfficeThemeCatalogRegistrar.Register();
                // ThemesLunaThemeCatalogRegistrar.Register();

                // Use the Actipro styles for native WPF controls that look great with Actipro's control products
                ThemeManager.AreNativeThemesEnabled = true;

                // Always force a Metro accented green theme (similar to Excel)
                ThemeManager.CurrentTheme = "MetroDark";
            }
            finally
            {
                ThemeManager.EndUpdate();
            }


            base.OnStartup(e);
        }
    }
}
