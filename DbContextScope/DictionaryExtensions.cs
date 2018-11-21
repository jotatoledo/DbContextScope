// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace EntityFrameworkCore.DbContextScope
{
    using System.Collections.Generic;

    /// <summary>
    /// Convenience extension methods for <see cref="IDictionary{TKey, TValue}"/>
    /// </summary>
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Returns the value associated with the specified key or the default value for <typeparamref name="TValue"/>
        /// </summary>
        /// <param name="dictionary">The dictionary instance</param>
        /// <param name="key">The dictionary key</param>
        /// <typeparam name="TKey">The type of keys in the dictionary</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary</typeparam>
        /// <returns>The value associated to the key or null if not present</returns>
        internal static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : default(TValue);
        }
    }
}
