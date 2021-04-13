using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Parse
{
    public class Helper
    {
        public static object _data;

        public static string GetDescription(object value)
        {
            // Get the Description attribute value for the enum value
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
    }


    public static class ReflectionHelper
    {
        // https://stackoverflow.com/a/13650728/37055
        public static object ReadProperty(this object target, string propertyName)
        {
            var args = new[] { CSharpArgumentInfo.Create(0, null) };
            var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, propertyName, target.GetType(), args);
            var site = CallSite<Func<CallSite, object, object>>.Create(binder);
            return site.Target(site, target);
        }

        public static string ReadDebuggerDisplay(this object target, string propertyName = "DebuggerDisplay")
        {
            string debuggerDisplay = null;
            try
            {
                var value = ReadProperty(target, propertyName) ?? "<null object>";

                debuggerDisplay = value as string ?? value.ToString();
            }
            catch (Exception)
            {
                // ignored
            }
            return debuggerDisplay ??
                  $"<ReadDebuggerDisplay failed on {target.GetType()}[{propertyName}]>";
        }
    }
}
