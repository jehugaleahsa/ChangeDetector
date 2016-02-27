using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    public class EntityChange<TEntity>
        where TEntity : class
    {
        private readonly IEnumerable<IPropertyChange> changes;

        internal EntityChange(
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

        public EntityChange<TDerived> As<TDerived>()
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
