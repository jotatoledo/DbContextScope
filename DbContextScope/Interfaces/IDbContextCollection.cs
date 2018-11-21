/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore;
#elif (NET45 || NET46)
using System.Data.Entity;
#endif
using System;

namespace EntityFrameworkCore.DbContextScope {
    /// <summary>
    /// Maintains a list of lazily-created DbContext instances.
    /// </summary>
    public interface IDbContextCollection : IDisposable {
        /// <summary>
        /// Get or create a DbContext instance of the specified type. 
        /// </summary>
        TDbContext Get<TDbContext>() where TDbContext : DbContext;
    }
}