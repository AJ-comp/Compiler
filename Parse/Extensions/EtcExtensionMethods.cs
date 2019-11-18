using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Parse.FrontEnd.Extensions
{
    public static class EtcExtensionMethods
    {
        public static string ToElementString<T>(this Stack<T> obj, string separator="")
        {
            Contract.Requires(obj != null);

            return string.Join(separator, obj);
        }

        public static T SecondItemPeek<T>(this Stack<T> obj)
        {
            Contract.Requires(obj != null);

            T result = default(T);
            if (obj.Count < 2) return result;

            T temp = obj.Pop();
            result = obj.Peek();
            obj.Push(temp);

            return result;
        }

        public static Stack<T> Clone<T>(this Stack<T> obj)
        {
            Contract.Requires(obj != null);

            return new Stack<T>(new Stack<T>(obj));
        }

        public static Stack<T> Reverse<T>(this Stack<T> obj)
        {
            Contract.Requires(obj != null);

            Stack<T> result = new Stack<T>();
            Stack<T> original = obj.Clone();

            while(original.Count > 0)   result.Push(original.Pop());

            return result;
        }
    }
}
