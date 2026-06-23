using System.Reflection;

namespace ApplicationLayer.Models.Invokers
{
    public class Invoker
    {
        // This can break MVVM pattern !! therefore must change another way. 
        public object target;
        public MethodInfo executer;

        public void Call(params object[] param) => executer?.Invoke(target, param);
    }
}
