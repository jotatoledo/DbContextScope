// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace EntityFrameworkCore.DbContextScope.Internal
{
#if NETSTANDARD2_0
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
#elif NET45 || NET46
    using System.Data.Entity;
#endif
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// As its name suggests, DbContextCollection maintains a collection of DbContext instances.
    ///
    /// What it does in a nutshell:
    /// - Lazily instantiates DbContext instances when its Get Of TDbContext () method is called
    /// (and optionally starts an explicit database transaction).
    /// - Keeps track of the DbContext instances it created so that it can return the existing
    /// instance when asked for a DbContext of a specific type.
    /// - Takes care of committing / rolling back changes and transactions on all the DbContext
    /// instances it created when its Commit() or Rollback() method is called.
    ///
    /// </summary>
    internal class DbContextCollection : IDbContextCollection
    {
        private readonly bool readOnly;
        private readonly Dictionary<Type, DbContext> initializedDbContexts;
#if NETSTANDARD2_0
        private readonly Dictionary<DbContext, IDbContextTransaction> transactions = new Dictionary<DbContext, IDbContextTransaction>();
#elif NET45 || NET46
        private readonly Dictionary<DbContext, DbContextTransaction> transactions = new Dictionary<DbContext, DbContextTransaction>();
#endif
        private readonly IsolationLevel? isolationLevel;
        private readonly IDbContextFactory dbContextFactory;
        private bool disposed;
        private bool completed;

        public DbContextCollection(bool readOnly = false, IsolationLevel? isolationLevel = null, IDbContextFactory dbContextFactory = null)
        {
            this.disposed = false;
            this.completed = false;

            this.initializedDbContexts = new Dictionary<Type, DbContext>();
            this.readOnly = readOnly;
            this.isolationLevel = isolationLevel;
            this.dbContextFactory = dbContextFactory;
        }

        internal IReadOnlyDictionary<Type, DbContext> InitializedDbContexts => this.initializedDbContexts;

        public TDbContext Get<TDbContext>()
            where TDbContext : DbContext
        {
            this.ThrowIfDisposed();

            var requestedType = typeof(TDbContext);

            if (!this.initializedDbContexts.ContainsKey(requestedType))
            {
                // First time we've been asked for this particular DbContext type.
                // Create one, cache it and start its database transaction if needed.
                var dbContext = this.dbContextFactory != null
                    ? this.dbContextFactory.CreateDbContext<TDbContext>()
                    : Activator.CreateInstance<TDbContext>();

                this.initializedDbContexts.Add(requestedType, dbContext);

                if (this.readOnly)
                {
#if NETSTANDARD2_0
                    dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
#elif NET45 || NET46
                    dbContext.Configuration.AutoDetectChangesEnabled = false;
#endif
                }

                if (this.isolationLevel.HasValue)
                {
                    var tran = dbContext.Database.BeginTransaction(this.isolationLevel.Value);
                    this.transactions.Add(dbContext, tran);
                }
            }

            return this.initializedDbContexts[requestedType] as TDbContext;
        }

        public int Commit()
        {
            this.ThrowIfDisposed();
            this.ThrowIfCompleted();

            // Best effort. You'll note that we're not actually implementing an atomic commit
            // here. It entirely possible that one DbContext instance will be committed successfully
            // and another will fail. Implementing an atomic commit would require us to wrap
            // all of this in a TransactionScope. The problem with TransactionScope is that
            // the database transaction it creates may be automatically promoted to a
            // distributed transaction if our DbContext instances happen to be using different
            // databases. And that would require the DTC service (Distributed Transaction Coordinator)
            // to be enabled on all of our live and dev servers as well as on all of our dev workstations.
            // Otherwise the whole thing would blow up at runtime.

            // In practice, if our services are implemented following a reasonably DDD approach,
            // a business transaction (i.e. a service method) should only modify entities in a single
            // DbContext. So we should never find ourselves in a situation where two DbContext instances
            // contain uncommitted changes here. We should therefore never be in a situation where the below
            // would result in a partial commit.
            ExceptionDispatchInfo lastError = null;

            var c = 0;

            foreach (var dbContext in this.initializedDbContexts.Values)
            {
                try
                {
                    if (!this.readOnly)
                    {
                        c += dbContext.SaveChanges();
                    }

                    this.CommitContextTransaction(dbContext);
                }
                catch (Exception e)
                {
                    lastError = ExceptionDispatchInfo.Capture(e);
                }
            }

            this.Finalize(lastError);

            return c;
        }

        public async Task<int> CommitAsync(CancellationToken cancelToken)
        {
            if (cancelToken == null)
            {
                throw new ArgumentNullException(nameof(cancelToken));
            }

            this.ThrowIfDisposed();
            this.ThrowIfCompleted();

            // See comments in the sync version of this method for more details.
            ExceptionDispatchInfo lastError = null;

            var c = 0;

            foreach (var dbContext in this.initializedDbContexts.Values)
            {
                try
                {
                    if (!this.readOnly)
                    {
                        c += await dbContext.SaveChangesAsync(cancelToken)
                            .ConfigureAwait(false);
                    }

                    this.CommitContextTransaction(dbContext);
                }
                catch (Exception e)
                {
                    lastError = ExceptionDispatchInfo.Capture(e);
                }
            }

            this.Finalize(lastError);

            return c;
        }

        public void Rollback()
        {
            this.ThrowIfDisposed();
            this.ThrowIfCompleted();

            ExceptionDispatchInfo lastError = null;

            foreach (var dbContext in this.initializedDbContexts.Values)
            {
                // There's no need to explicitly rollback changes in a DbContext as
                // DbContext doesn't save any changes until its SaveChanges() method is called.
                // So "rolling back" for a DbContext simply means not calling its SaveChanges()
                // method.

                // But if we've started an explicit database transaction, then we must roll it back.
                var tran = this.transactions.GetValueOrDefault(dbContext);
                if (tran != null)
                {
                    try
                    {
                        tran.Rollback();
                        tran.Dispose();
                    }
                    catch (Exception e)
                    {
                        lastError = ExceptionDispatchInfo.Capture(e);
                    }
                }
            }

            this.Finalize(lastError);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            // Do our best here to dispose as much as we can even if we get errors along the way.
            // Now is not the time to throw. Correctly implemented applications will have called
            // either Commit() or Rollback() first and would have got the error there.
            if (!this.completed)
            {
                try
                {
                    if (this.readOnly)
                    {
                        this.Commit();
                    }
                    else
                    {
                        this.Rollback();
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            foreach (var dbContext in this.initializedDbContexts.Values)
            {
                try
                {
                    dbContext.Dispose();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            this.initializedDbContexts.Clear();
            this.disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(DbContextCollection));
            }
        }

        private void ThrowIfCompleted()
        {
            if (this.completed)
            {
                throw new InvalidOperationException(
                    @"You can't call Commit() or Rollback() more than once on a DbContextCollection. 
                    All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. 
                    If you wish to make more data changes, create a new DbContextCollection and make your changes there.");
            }
        }

        private void Finalize(ExceptionDispatchInfo lastError)
        {
            this.transactions.Clear();
            this.completed = true;

            if (lastError != null)
            {
                // Re-throw while maintaining the exception's original stack track
                lastError.Throw();
            }
        }

        /// <summary>
        /// Commits and disposes a transaction associated to a given context, if existent.
        /// Otherwise NO-OP.
        /// </summary>
        /// <param name="dbContext">The context instance</param>
        private void CommitContextTransaction(DbContext dbContext)
        {
            // If we've started an explicit database transaction, time to commit it now.
            var tran = this.transactions.GetValueOrDefault(dbContext);
            if (tran != null)
            {
                tran.Commit();
                tran.Dispose();
            }
        }
    }
}