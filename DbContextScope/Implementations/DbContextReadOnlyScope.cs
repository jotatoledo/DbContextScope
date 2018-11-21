// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace EntityFrameworkCore.DbContextScope
{
    using System.Data;

    public class DbContextReadOnlyScope : IDbContextReadOnlyScope
    {
        private readonly DbContextScope internalScope;

        public DbContextReadOnlyScope(IDbContextFactory dbContextFactory = null)
            : this(joiningOption: DbContextScopeOption.JoinExisting, isolationLevel: null, dbContextFactory: dbContextFactory) { }

        public DbContextReadOnlyScope(IsolationLevel isolationLevel, IDbContextFactory dbContextFactory = null)
            : this(joiningOption: DbContextScopeOption.ForceCreateNew, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory) { }

        public DbContextReadOnlyScope(DbContextScopeOption joiningOption, IsolationLevel? isolationLevel, IDbContextFactory dbContextFactory = null)
        {
            this.internalScope = new DbContextScope(joiningOption: joiningOption, readOnly: true, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory);
        }

        public IDbContextCollection DbContexts => this.internalScope.DbContexts;

        public void Dispose()
        {
            this.internalScope.Dispose();
        }
    }
}