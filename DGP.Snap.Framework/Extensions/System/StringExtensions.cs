using System;
using System.ComponentModel;

namespace DGP.Snap.Framework.Extensions.System
{
    public static class StringExtensions
    {
        public static T? Value<T>(this string input)
        {
            try
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
            }
            catch
            {
                return default;
            }
        }

        public static object? Value(this string input, Type type)
        {
            try
            {
                return TypeDescriptor.GetConverter(type).ConvertFromString(input);
            }
            catch
            {
                return null;
            }
        }
    }
}
