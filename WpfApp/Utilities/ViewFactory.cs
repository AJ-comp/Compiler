using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp.Utilities
{
    public static class ViewFactory
    {
        private static Dictionary<Type, UserControl> _viewCache = new Dictionary<Type, UserControl>();
        public static UserControl GetView(Type type)
        {
            if (_viewCache.ContainsKey(type) == false)
            {
                var userControl = Activator.CreateInstance(type) as UserControl;

                if (userControl == null)
                {
                    throw new InvalidOperationException("Couldn't create user control" + type);
                }

                _viewCache.Add(type, userControl);
            }

            return _viewCache[type];
        }
    }
}
