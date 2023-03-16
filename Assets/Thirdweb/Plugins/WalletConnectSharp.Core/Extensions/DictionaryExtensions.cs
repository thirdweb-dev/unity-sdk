using System;
using System.Collections.Generic;

namespace WalletConnectSharp.Core.Extensions
{
    public static class DictionaryExtensions
    {
        /// <Summary>
        /// Try to add the element to the dictionary if the key does not already exist.
        /// </Summary>
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
    }
}
