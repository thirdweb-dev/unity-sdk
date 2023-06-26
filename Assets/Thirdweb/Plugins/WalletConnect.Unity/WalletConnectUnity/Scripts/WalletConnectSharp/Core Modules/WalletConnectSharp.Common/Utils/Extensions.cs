using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace WalletConnectSharp.Common.Utils
{
    /// <summary>
    /// General purpose extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert an anonymous type to a Dictionary
        /// </summary>
        /// <param name="obj">The anonymous type instance to convert to a dictionary</param>
        /// <param name="enforceLowercase">Enforce all keys to be lowercased</param>
        /// <returns>A dictionary where each key is the property name of the anonymous type
        /// and each value is the property's value</returns>
        public static Dictionary<string, object> AsDictionary(this object obj, bool enforceLowercase = true)
        {
            if (obj is Dictionary<string, object> objects)
                return objects;
            
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (obj != null)
            {
                foreach (PropertyInfo propertyDescriptor in obj.GetType().GetProperties())
                {
                    object value = propertyDescriptor.GetValue(obj, null);
                    var key = enforceLowercase ? propertyDescriptor.Name.ToLower() : propertyDescriptor.Name;
                    
                    dict.Add(key, value);
                }
            }

            return dict;
        }
        
        /// <summary>
        /// Returns true if the given object is a numeric type
        /// </summary>
        /// <param name="o">The object to check</param>
        /// <returns>Returns true if the object has a numeric type</returns>
        public static bool IsNumericType(this object o)
        {   
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Add a query parameter to the given source string
        /// </summary>
        /// <param name="source">The source string to add the generated query parameter to</param>
        /// <param name="key">The key of the query parameter</param>
        /// <param name="value">The value of the query parameter</param>
        /// <returns>The original source string with the generated query parameter appended</returns>
        public static string AddQueryParam(
            this string source, string key, string value)
        {
            string delim;
            if ((source == null) || !source.Contains("?"))
            {
                delim = "?";
            }
            else if (source.EndsWith("?") || source.EndsWith("&"))
            {
                delim = string.Empty;
            }
            else
            {
                delim = "&";
            }

            return source + delim + HttpUtility.UrlEncode(key)
                   + "=" + HttpUtility.UrlEncode(value);
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeout = 1000, string message = "Timeout of %t exceeded")
        {
            var resultT = await Task.WhenAny(task, Task.Delay(timeout));
            if (resultT != task)
            {
                throw new TimeoutException(message.Replace("%t", timeout.ToString()));
            }

            return ((Task<T>) resultT).Result;
        }
        
        public static async Task WithTimeout(this Task task, int timeout = 1000, string message = "Timeout of %t exceeded")
        {
            var resultT = await Task.WhenAny(task, Task.Delay(timeout));
            if (resultT != task)
            {
                throw new TimeoutException(message.Replace("%t", timeout.ToString()));
            }
        }
        
        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, string message = "Timeout of %t exceeded")
        {
            var resultT = await Task.WhenAny(task, Task.Delay(timeout));
            if (resultT != task)
            {
                throw new TimeoutException(message.Replace("%t", timeout.ToString()));
            }

            return ((Task<T>) resultT).Result;
        }
        
        public static async Task WithTimeout(this Task task, TimeSpan timeout, string message = "Timeout of %t exceeded")
        {
            var resultT = await Task.WhenAny(task, Task.Delay(timeout));
            if (resultT != task)
            {
                throw new TimeoutException(message.Replace("%t", timeout.ToString()));
            }
        }
        
        public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second,
            IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(second, comparer ?? EqualityComparer<T>.Default)
                .SetEquals(first);
        }
    }
}
