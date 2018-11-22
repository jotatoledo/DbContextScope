// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace Numero3.EntityFramework.Demo.BusinessLogicServices
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EntityFrameworkCore.DbContextScope;
    using Numero3.EntityFramework.Demo.DatabaseContext;

    public class UserCreditScoreService
    {
        private readonly IDbContextScopeFactory dbContextScopeFactory;

        public UserCreditScoreService(IDbContextScopeFactory dbContextScopeFactory)
        {
            this.dbContextScopeFactory = dbContextScopeFactory ?? throw new ArgumentNullException("dbContextScopeFactory");
        }

        public void UpdateCreditScoreForAllUsers()
        {
            /*
             * Demo of DbContextScope + parallel programming.
             */

            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                //-- Get all users
                var dbContext = dbContextScope.DbContexts.Get<UserManagementDbContext>();
                var userIds = dbContext.Users.Select(u => u.Id).ToList();

                Console.WriteLine("Found {0} users in the database. Will calculate and store their credit scores in parallel.", userIds.Count);

                //-- Calculate and store the credit score of each user
                // We're going to imagine that calculating a credit score of a user takes some time.
                // So we'll do it in parallel.

                // You MUST call SuppressAmbientContext() when kicking off a parallel execution flow
                // within a DbContextScope. Otherwise, this DbContextScope will remain the ambient scope
                // in the parallel flows of execution, potentially leading to multiple threads
                // accessing the same DbContext instance.
                using (this.dbContextScopeFactory.SuppressAmbientContext())
                {
                    Parallel.ForEach(userIds, this.UpdateCreditScore);
                }

                // Note: SaveChanges() isn't going to do anything in this instance since all the changes
                // were actually made and saved in separate DbContextScopes created in separate threads.
                dbContextScope.SaveChanges();
            }
        }

        public void UpdateCreditScore(Guid userId)
        {
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<UserManagementDbContext>();
                var user = dbContext.Users.Find(userId);
                if (user == null)
                {
                    throw new ArgumentException(String.Format("Invalid userId provided: {0}. Couldn't find a User with this ID.", userId));
                }

                // Simulate the calculation of a credit score taking some time
                var random = new Random(Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(random.Next(300, 1000));

                user.CreditScore = random.Next(1, 100);
                dbContextScope.SaveChanges();
            }
        }
    }
}
