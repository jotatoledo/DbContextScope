// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace EntityFrameworkCore.DbContextScope
{
#if NETSTANDARD2_0
    using System.Collections.Concurrent;
    using System.Threading;
#elif NET45 || NET46
    using System.Runtime.Remoting.Messaging;
#endif

    /// <summary>
    /// Provides a way to set contextual data that flows with the call and
    /// async context of a test or invocation.
    /// http://www.cazzulino.com/callcontext-netstandard-netcore.html
    /// </summary>
    internal static class ContextScopeCallContext
    {
#if NETSTANDARD2_0
        private static ConcurrentDictionary<string, AsyncLocal<object>> state = new ConcurrentDictionary<string, AsyncLocal<object>>();
#endif

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, object data) => InternalSet(name, data);

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="ContextScopeCallContext"/>.
        /// </summary>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or <see langword="null"/> if not found.</returns>
        public static object GetData(string name) => InternalTryGet(name);

#if NETSTANDARD2_0
        private static void InternalSet(string name, object data)
        {
            state.GetOrAdd(name, _ => new AsyncLocal<object>()).Value = data;
        }

        private static object InternalTryGet(string name)
        {
            return state.TryGetValue(name, out AsyncLocal<object> data) ? data.Value : null;
        }
#elif NET45 || NET46
        private static void InternalSet(string name, object data)
        {
            CallContext.LogicalSetData(name, data);
        }

        private static object InternalTryGet(string name)
        {
            return CallContext.LogicalGetData(name);
        }
#endif
    }
}