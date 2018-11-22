// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace Numero3.EntityFramework.Demo.Repositories
{
    using System;
    using System.Threading.Tasks;
    using EntityFrameworkCore.DbContextScope;
    using Numero3.EntityFramework.Demo.DatabaseContext;
    using Numero3.EntityFramework.Demo.DomainModel;

    /*
     * An example "repository" relying on an ambient DbContext instance.
     *
     * Since we use EF to persist our data, the actual repository is of course the EF DbContext. This
     * class is called a "repository" for old time's sake but is merely just a collection
     * of pre-built Linq-to-Entities queries. This avoids having these queries copied and
     * pasted in every service method that need them and facilitates unit testing.
     *
     * Whether your application would benefit from using this additional layer or would
     * be better off if its service methods queried the DbContext directly or used some sort of query
     * object pattern is a design decision for you to make.
     *
     * DbContextScope is agnostic to this and will happily let you use any approach you
     * deem most suitable for your application.
     *
     */
    public class UserRepository : IUserRepository
    {
        private readonly IAmbientDbContextLocator ambientDbContextLocator;

        public UserRepository(IAmbientDbContextLocator ambientDbContextLocator)
        {
            this.ambientDbContextLocator = ambientDbContextLocator ?? throw new ArgumentNullException("ambientDbContextLocator");
        }

        private UserManagementDbContext DbContext
        {
            get
            {
                var dbContext = this.ambientDbContextLocator.Get<UserManagementDbContext>();

                if (dbContext == null)
                {
                    throw new InvalidOperationException("No ambient DbContext of type UserManagementDbContext found. This means that this repository method has been called outside of the scope of a DbContextScope. A repository must only be accessed within the scope of a DbContextScope, which takes care of creating the DbContext instances that the repositories need and making them available as ambient contexts. This is what ensures that, for any given DbContext-derived type, the same instance is used throughout the duration of a business transaction. To fix this issue, use IDbContextScopeFactory in your top-level business logic service method to create a DbContextScope that wraps the entire business transaction that your service method implements. Then access this repository within that scope. Refer to the comments in the IDbContextScope.cs file for more details.");
                }

                return dbContext;
            }
        }

        public User Get(Guid userId)
        {
            return this.DbContext.Users.Find(userId);
        }

        public Task<User> GetAsync(Guid userId)
        {
            return this.DbContext.Users.FindAsync(userId);
        }

        public void Add(User user)
        {
            this.DbContext.Users.Add(user);
        }
    }
}
