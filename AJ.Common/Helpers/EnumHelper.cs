using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace AJ.Common.Helpers
{
    public static class EnumHelper
    {
        public static T Parse<T>(this Enum obj, string s)
        {
            return (T)Enum.Parse(typeof(T), s);
        }


        /*****************************************************/
        /// <summary>
        /// <para>This function gets Description attribute value of Enum.</para>
        /// <para>Enum의 Description attribute 값을 가져옵니다.</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /*****************************************************/
        public static string ToDescription(this Enum value)
        {
            // Get the Description attribute value for the enum value
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
    }
}
