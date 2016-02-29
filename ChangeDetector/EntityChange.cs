using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    public interface IEntityChange<TEntity>
    {
        TEntity Entity { get; }

        EntityState State { get; }

        object Data { get; }

        IEnumerable<IPropertyChange> GetChanges();

        IPropertyChange GetChange<TProp>(Expression<Func<TEntity, TProp>> accessor);

        bool HasChange<TProp>(Expression<Func<TEntity, TProp>> accessor);

        IEntityChange<TDerived> As<TDerived>() where TDerived : class, TEntity;
    }

    internal class EntityChange<TEntity> : IEntityChange<TEntity>
        where TEntity : class
    {
        private readonly IEnumerable<IPropertyChange> changes;

        public EntityChange(
            TEntity entity,
            EntityState state,
            object data,
            IEnumerable<IPropertyChange> changes)
        {
            this.Entity = entity;
            this.State = state;
            this.Data = data;
            this.changes = changes;
        }

        public TEntity Entity { get; private set; }

        public EntityState State { get; private set; }

        public object Data { get; private set; }

        public IEnumerable<IPropertyChange> GetChanges()
        {
            return changes;
        }

        public IPropertyChange GetChange<TProp>(Expression<Func<TEntity, TProp>> accessor)
        {
            return getChange<TProp>(accessor);
        }

        public bool HasChange<TProp>(Expression<Func<TEntity, TProp>> accessor)
        {
            return getChange(accessor) != null;
        }

        private IPropertyChange getChange<TProp>(Expression<Func<TEntity, TProp>> accessor)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            if (property == null)
            {
                return null;
            }
            return changes.Where(c => c.Property == property).SingleOrDefault();
        }

        public IEntityChange<TDerived> As<TDerived>()
            where TDerived : class, TEntity
        {
            EntityChange<TDerived> change = new EntityChange<TDerived>(
                Entity as TDerived,
                State,
                Data,
                changes);
            return change;
        }
    }
}
