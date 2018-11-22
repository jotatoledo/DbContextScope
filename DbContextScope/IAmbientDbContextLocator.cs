// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace EntityFrameworkCore.DbContextScope
{
#if NETSTANDARD2_0
    using Microsoft.EntityFrameworkCore;
#elif NET45 || NET46
    using System.Data.Entity;
#endif

    /// <summary>
    /// Convenience methods to retrieve ambient DbContext instances.
    /// </summary>
    public interface IAmbientDbContextLocator
    {
        /// <summary>
        /// If called within the scope of a DbContextScope, gets or creates
        /// the ambient DbContext instance for the provided DbContext type.
        /// Otherwise returns null.
        /// </summary>
        /// <typeparam name="TDbContext">The context concrete type</typeparam>
        /// <returns>A context instance</returns>
        TDbContext Get<TDbContext>()
            where TDbContext : DbContext;
    }
}
