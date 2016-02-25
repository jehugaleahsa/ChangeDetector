using System;
using System.Collections.Generic;
using System.Linq;

namespace ChangeDetector
{
    public class EntityChangeTracker<TEntity>
        where TEntity : class
    {
        private readonly EntityConfiguration<TEntity> configuration;
        private readonly Dictionary<TEntity, Entity<TEntity>> entityLookup;

        public EntityChangeTracker(EntityConfiguration<TEntity> configuration)
            : this(configuration, null)
        {
        }

        public EntityChangeTracker(EntityConfiguration<TEntity> configuration, IEqualityComparer<TEntity> comparer)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (comparer == null)
            {
                comparer = EqualityComparer<TEntity>.Default;
            }
            this.configuration = configuration;
            this.entityLookup = new Dictionary<TEntity, Entity<TEntity>>(comparer);
        }

        // Added -> Unmodified
        // Removed -> Unmodified
        // Unmodified -> Unmodified
        // Detached -> Unmodified
        public void Attach(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            Entity<TEntity> context;
            if (entityLookup.TryGetValue(entity, out context))
            {
                if (context.State == EntityState.Added)
                {
                    context.State = EntityState.Unmodified;
                }
                else if (context.State == EntityState.Removed)
                {
                    context.State = EntityState.Unmodified;
                }
            }
            else
            {
                registerEntity(entity, EntityState.Unmodified);
            }
        }

        // Added -> Detached
        // Removed -> Detached
        // Unmodified -> Detached
        // Detached -> Detached
        public bool Detach(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            return entityLookup.Remove(entity);
        }

        // Added -> Added
        // Removed -> Unmodified
        // Unmodified -> Unmodified
        // Detached -> Added
        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            Entity<TEntity> context;
            if (entityLookup.TryGetValue(entity, out context))
            {
                if (context.State == EntityState.Removed)
                {
                    context.State = EntityState.Unmodified;
                }
            }
            else
            {
                registerEntity(entity, EntityState.Added);
            }
        }

        private void registerEntity(TEntity entity, EntityState state)
        {
            Entity<TEntity> context = new Entity<TEntity>();
            context.Instance = entity;
            context.State = state;
            context.Snapshot = configuration.TakeSnapshot(entity);
            entityLookup.Add(entity, context);
        }

        // Added -> Detached
        // Removed -> Removed
        // Unmodified -> Removed
        // Detached -> Detached
        public bool Remove(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            Entity<TEntity> context;
            if (!entityLookup.TryGetValue(entity, out context))
            {
                return false;
            }
            if (context.State == EntityState.Added)
            {
                return entityLookup.Remove(context.Instance);
            }
            if (context.State == EntityState.Unmodified)
            {
                context.State = EntityState.Removed;
            }
            return true;
        }

        public IEnumerable<EntityChange<TEntity>> DetectChanges()
        {
            return detectChanges(EntityState.Added | EntityState.Modified | EntityState.Removed);
        }

        public IEnumerable<EntityChange<TEntity>> DetectChanges(EntityState state)
        {
            return detectChanges(state);
        }

        private IEnumerable<EntityChange<TEntity>> detectChanges(EntityState state)
        {
            List<EntityChange<TEntity>> changes = new List<EntityChange<TEntity>>();
            foreach (Entity<TEntity> context in entityLookup.Values)
            {
                if (context.State == EntityState.Added && state.HasFlag(EntityState.Added))
                {
                    EntityChange<TEntity> change = getEntityChange(context);
                    changes.Add(change);
                }
                else if (context.State == EntityState.Unmodified && (state.HasFlag(EntityState.Modified) || state.HasFlag(EntityState.Unmodified)))
                {
                    EntityChange<TEntity> change = getEntityChange(context);
                    if (change.PropertyChanges.Any())
                    {
                        if (state.HasFlag(EntityState.Modified))
                        {
                            changes.Add(change);
                        }
                    }
                    else if (state.HasFlag(EntityState.Unmodified))
                    {
                        changes.Add(change);
                    }
                }
                else if (context.State == EntityState.Removed && state.HasFlag(EntityState.Removed))
                {
                    EntityChange<TEntity> change = getEntityChange(context);
                    changes.Add(change);
                }
            }
            return changes;
        }

        public EntityChange<TEntity> DetectChanges(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            Entity<TEntity> context;
            if (entityLookup.TryGetValue(entity, out context))
            {
                return getEntityChange(context);
            }
            else
            {
                EntityChange<TEntity> change = new EntityChange<TEntity>();
                change.Entity = entity;
                change.State = EntityState.Detached;
                change.PropertyChanges = new IPropertyChange[0];
                return change;
            }
        }

        private EntityChange<TEntity> getEntityChange(Entity<TEntity> context)
        {
            var propertyChanges = getPropertyChanges(context);
            EntityChange<TEntity> change = new EntityChange<TEntity>();
            change.Entity = context.Instance;
            change.State = getState(context.State, propertyChanges);
            change.PropertyChanges = propertyChanges;
            return change;
        }

        private IEnumerable<IPropertyChange> getPropertyChanges(Entity<TEntity> context)
        {
            if (context.State == EntityState.Added)
            {
                var snapshot = configuration.TakeSnapshot(context.Instance);
                return configuration.GetChanges(Snapshot.Null, snapshot);
            }
            else if (context.State == EntityState.Removed)
            {
                var snapshot = configuration.TakeSnapshot(context.Instance);
                return configuration.GetChanges(snapshot, Snapshot.Null);
            }
            else if (context.State == EntityState.Unmodified)
            {
                var snapshot = configuration.TakeSnapshot(context.Instance);
                var changes = configuration.GetChanges(context.Snapshot, snapshot);
                return changes;
            }
            else
            {
                return new IPropertyChange[0];
            }
        }

        private static EntityState getState(EntityState entityState, IEnumerable<IPropertyChange> changes)
        {
            if (entityState == EntityState.Added)
            {
                return EntityState.Added;
            }
            else if (entityState == EntityState.Removed)
            {
                return EntityState.Removed;
            }
            else if (entityState == EntityState.Unmodified)
            {
                if (changes.Any())
                {
                    return EntityState.Modified;
                }
                else
                {
                    return EntityState.Unmodified;
                }
            }
            else
            {
                return EntityState.Detached;
            }
        }

        public void CommitChanges()
        {
            commitChanges(EntityState.Added | EntityState.Modified | EntityState.Removed);
        }

        public void CommitChanges(EntityState state)
        {
            commitChanges(state);
        }

        private void commitChanges(EntityState state)
        {
            HashSet<TEntity> removals = new HashSet<TEntity>(entityLookup.Comparer);
            foreach (Entity<TEntity> context in entityLookup.Values)
            {
                if (context.State == EntityState.Added && state.HasFlag(EntityState.Added))
                {
                    context.State = EntityState.Unmodified;
                    context.Snapshot = configuration.TakeSnapshot(context.Instance);
                }
                else if (context.State == EntityState.Unmodified && (state.HasFlag(EntityState.Modified) || state.HasFlag(EntityState.Unmodified)))
                {
                    context.Snapshot = configuration.TakeSnapshot(context.Instance);
                }
                else if (context.State == EntityState.Removed && state.HasFlag(EntityState.Removed))
                {
                    removals.Add(context.Instance);
                }
            }
            foreach (TEntity entity in removals)
            {
                entityLookup.Remove(entity);
            }
        }

        public void CommitChanges(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            Entity<TEntity> context;
            if (!entityLookup.TryGetValue(entity, out context))
            {
                return;
            }
            if (context.State == EntityState.Added)
            {
                context.State = EntityState.Unmodified;
                context.Snapshot = configuration.TakeSnapshot(context.Instance);
            }
            else if (context.State == EntityState.Unmodified)
            {
                context.Snapshot = configuration.TakeSnapshot(context.Instance);
            }
            else if (context.State == EntityState.Removed)
            {
                entityLookup.Remove(entity);
            }
        }
    }
}
