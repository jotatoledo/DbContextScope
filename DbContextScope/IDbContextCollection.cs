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
    using System;

    /// <summary>
    /// Maintains a list of lazily-created <see cref="DbContext"/> instances.
    /// </summary>
    public interface IDbContextCollection : IDisposable
    {
        /// <summary>
        /// Get or create a DbContext instance of the specified type.
        /// </summary>
        /// <typeparam name="TDbContext">The context type</typeparam>
        /// <returns>A context instance</returns>
        TDbContext Get<TDbContext>()
            where TDbContext : DbContext;
    }
}