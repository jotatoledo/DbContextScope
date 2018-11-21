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

namespace EntityFrameworkCore.DbContextScope {
    public class AmbientDbContextLocator : IAmbientDbContextLocator {
        public TDbContext Get<TDbContext>() where TDbContext : DbContext {
            var ambientDbContextScope = DbContextScope.GetAmbientScope();
            return ambientDbContextScope?.DbContexts.Get<TDbContext>();
        }
    }
}