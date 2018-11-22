// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace Numero3.EntityFramework.Demo.DatabaseContext
{
    using Microsoft.EntityFrameworkCore;
    using Numero3.EntityFramework.Demo.DomainModel;

    public class UserManagementDbContext : DbContext
    {
        // Map our 'User' model by convention
        public DbSet<User> Users { get; set; }

        public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(builder =>
            {
                builder.Property(m => m.Name).IsRequired();
                builder.Property(m => m.Email).IsRequired();
            });
        }
    }
}
