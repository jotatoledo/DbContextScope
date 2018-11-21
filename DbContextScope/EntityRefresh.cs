namespace EntityFrameworkCore.DbContextScope
{
    using System.Linq;
#if NETSTANDARD2_0
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
#elif (NET45 || NET46)
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
#endif

    internal sealed class EntityRefresh : IEntityRefresh
    {
        private readonly DbContext contextInCurrentScope;
        private readonly DbContext correspondingParentContext;

        public EntityRefresh(DbContext contextInCurrentScope, DbContext correspondingParentContext)
        {
            this.contextInCurrentScope = contextInCurrentScope;
            this.correspondingParentContext = correspondingParentContext;
        }

        public void Refresh<TEntity>(TEntity toRefresh)
        {
#if NETSTANDARD2_0
            // First, we need to find what the EntityKey for this entity is. 
            // We need this EntityKey in order to check if this entity has
            // already been loaded in the parent DbContext's first-level cache (the ObjectStateManager).
            var stateInCurrentScope = contextInCurrentScope.ChangeTracker
                .GetInfrastructure()
                .TryGetEntry(toRefresh);
            if (stateInCurrentScope != null)
            {
                // NOTE(tim): Thanks to ninety7 (https://github.com/ninety7/DbContextScope) and apawsey (https://github.com/apawsey/DbContextScope)
                // for examples on how identify the matching entities in EF Core.
                var entityType = stateInCurrentScope.Entity.GetType();
                var key = stateInCurrentScope.EntityType.FindPrimaryKey();
                var keyValues = key.Properties
                    .Select(s => entityType.GetProperty(s.Name).GetValue(stateInCurrentScope.Entity))
                    .ToArray();

                // Now we can see if that entity exists in the parent DbContext instance and refresh it.
                var stateInParentScope = correspondingParentContext.ChangeTracker
                    .GetInfrastructure()
                    .TryGetEntry(key, keyValues);
                if (stateInParentScope != null)
                {
                    // Only refresh the entity in the parent DbContext from the database if that entity hasn't already been
                    // modified in the parent. Otherwise, let the whatever concurency rules the application uses
                    // apply.
                    if (stateInParentScope.EntityState == EntityState.Unchanged)
                    {
                        correspondingParentContext.Entry(stateInParentScope.Entity).Reload();
                    }
                }
            }
#elif (NET45 || NET46)
            // First, we need to find what the EntityKey for this entity is. 
            // We need this EntityKey in order to check if this entity has
            // already been loaded in the parent DbContext's first-level cache (the ObjectStateManager).
            var objectContext = (contextInCurrentScope as IObjectContextAdapter).ObjectContext;
            var stateManager = objectContext.ObjectStateManager;
            if (stateManager.TryGetObjectStateEntry(toRefresh, out ObjectStateEntry stateInCurrentScope))
            {
                var key = stateInCurrentScope.EntityKey;

                // Now we can see if that entity exists in the parent DbContext instance and refresh it.
                if (stateManager.TryGetObjectStateEntry(key, out ObjectStateEntry stateInParentScope))
                {
                    // Only refresh the entity in the parent DbContext from the database if that entity hasn't already been
                    // modified in the parent. Otherwise, let the whatever concurency rules the application uses
                    // apply.
                    if (stateInParentScope.State == EntityState.Unchanged)
                    {
                        objectContext.Refresh(RefreshMode.StoreWins, stateInParentScope.Entity);
                    }
                }
            }
#endif
        }
    }
}
