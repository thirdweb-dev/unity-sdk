using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cdm.Authentication.Utils
{
    public static class JsonHelper
    {
        public static Dictionary<string, string> ToDictionary(object obj)
        {
            var dictionary = JObject.FromObject(obj).ToObject<Dictionary<string, string>>();
            
            if (dictionary != null)
            {
                // Remove empty parameters.
                var keys = dictionary.Keys.Where(key => string.IsNullOrEmpty(dictionary[key])).ToArray();
                foreach (var key in keys)
                {
                    dictionary.Remove(key);
                }    
            }

            return dictionary;
        }

        public static T FromDictionary<T>(Dictionary<string, string> dictionary)
        {
            return JObject.FromObject(dictionary).ToObject<T>();
        }
        
        public static bool TryGetFromDictionary<T>(Dictionary<string, string> dictionary, out T value)
        {
            try
            {
                value = FromDictionary<T>(dictionary);
                return true;
            }
            catch (JsonSerializationException)
            {
                // ignored
            }

            value = default;
            return false;
        }
        
        public static bool TryGetFromNameValueCollection<T>(NameValueCollection collection, out T value)
        {
            var dictionary = new Dictionary<string, string>();
            
            foreach (string s in collection)
            {
                dictionary.Add(s, collection[s]);
            }
            
            return TryGetFromDictionary<T>(dictionary, out value);
        }
    }
}