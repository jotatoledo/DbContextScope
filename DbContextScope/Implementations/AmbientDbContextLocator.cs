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

    public class AmbientDbContextLocator : IAmbientDbContextLocator
    {
        public TDbContext Get<TDbContext>() where TDbContext : DbContext
        {
            var ambientDbContextScope = DbContextScope.GetAmbientScope();
            return ambientDbContextScope?.DbContexts.Get<TDbContext>();
        }
    }
}