using System;
using System.Collections;
using System.Windows;

namespace ApplicationLayer.Views
{
    public class MenusForTarget : FrameworkElement
    {
        public Type TargetType
        {
            get { return (Type)GetValue(TargetTypeProperty); }
            set { SetValue(TargetTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(MenusForTarget), new PropertyMetadata(null));


        public ArrayList Menu
        {
            get { return (ArrayList)GetValue(MenuProperty); }
            set { SetValue(MenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Menu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MenuProperty =
            DependencyProperty.Register("Menu", typeof(ArrayList), typeof(MenusForTarget), new PropertyMetadata(null));

    }
}
