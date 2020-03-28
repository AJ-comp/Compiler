using GraphSharp.Algorithms.Layout;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using System.Windows;

namespace ApplicationLayer.Views.AttachedProperties
{
    public static class LayoutParametersHook
    {


        public static double GetWidthPerHeightHook(DependencyObject obj)
        {
            return (double)obj.GetValue(WidthPerHeightHookProperty);
        }

        public static void SetWidthPerHeightHook(DependencyObject obj, double value)
        {
            obj.SetValue(WidthPerHeightHookProperty, value);
        }

        // Using a DependencyProperty as the backing store for WidthPerHeightHook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthPerHeightHookProperty =
            DependencyProperty.RegisterAttached("WidthPerHeightHook", typeof(double), typeof(LayoutParametersHook), new PropertyMetadata(0.0, OnWidthPerHeightChanged));


        private static void OnWidthPerHeightChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var widthPerHeight = (sender as ILayoutParameters);
            if (widthPerHeight == null) return;

            double value = (double)e.NewValue;
            widthPerHeight = new SimpleTreeLayoutParameters();
            (widthPerHeight as SimpleTreeLayoutParameters).WidthPerHeight = value;
        }

    }
}
