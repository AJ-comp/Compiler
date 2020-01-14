using ApplicationLayer.Models.Invokers;
using System.Reflection;
using System.Windows;
using System.Windows.Interactivity;

namespace ApplicationLayer.WpfApp.Behaviors
{
    public class ViewViewModelConnector : Behavior<FrameworkElement>
    {
        public Invoker ViewModelDelegate
        {
            get { return (Invoker)GetValue(ViewModelDelegateProperty); }
            set { SetValue(ViewModelDelegateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModelDelegate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelDelegateProperty =
            DependencyProperty.Register("ViewModelDelegate", typeof(Invoker), typeof(ViewViewModelConnector), new PropertyMetadata(null));


        public string ViewMethod
        {
            get { return (string)GetValue(ViewMethodProperty); }
            set { SetValue(ViewMethodProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewMethod.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewMethodProperty =
            DependencyProperty.Register("ViewMethod", typeof(string), typeof(ViewViewModelConnector), new PropertyMetadata(string.Empty));


        //private static Delegate CreateDelegate(this MethodInfo methodInfo, object target)
        //{
        //    Func<Type[], Type> getType;
        //    var isAction = methodInfo.ReturnType.Equals((typeof(void)));
        //    var types = methodInfo.GetParameters().Select(p => p.ParameterType);

        //    if (isAction)
        //    {
        //        getType = Expression.GetActionType;
        //    }
        //    else
        //    {
        //        getType = Expression.GetFuncType;
        //        types = types.Concat(new[] { methodInfo.ReturnType });
        //    }

        //    if (methodInfo.IsStatic)
        //    {
        //        return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
        //    }

        //    return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        //}

        protected override void OnAttached()
        {
            if (ViewModelDelegate == null) return;

            // connect new method
            if (!string.IsNullOrEmpty(this.ViewMethod))
            {
                var type = this.AssociatedObject.GetType();

                var mi = this.AssociatedObject.GetType().GetMethod(this.ViewMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                this.ViewModelDelegate.executer = mi;
                this.ViewModelDelegate.target = this.AssociatedObject;
            }
        }
    }
}
