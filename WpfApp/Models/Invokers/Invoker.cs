using System;
using System.Reflection;

namespace WpfApp.Models.Invokers
{
    public class Invoker
    {
        // This can break MVVM pattern !! therefore must change another way. 
        internal object target;
        internal MethodInfo executer;

        public void Call(params object[] param) => executer?.Invoke(target, param);
    }
}
