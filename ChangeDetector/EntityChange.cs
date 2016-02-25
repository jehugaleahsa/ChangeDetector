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
        public TEntity Entity { get; internal set; }

        public EntityState State { get; internal set; }

        public IEnumerable<IFieldChange> FieldChanges { get; internal set; }

        public IFieldChange GetChange<TProp>(Expression<Func<TEntity, TProp>> accessor)
        {
            return getChange<TProp>(accessor);
        }

        public bool HasChange<TProp>(Expression<Func<TEntity, TProp>> accessor)
        {
            return getChange(accessor) != null;
        }

        private IFieldChange getChange<TProp>(Expression<Func<TEntity, TProp>> accessor)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            if (property == null)
            {
                return null;
            }
            return FieldChanges.Where(c => c.Property == property).SingleOrDefault();
        }

        public EntityChange<TDerived> As<TDerived>()
            where TDerived : class, TEntity
        {
            EntityChange<TDerived> change = new EntityChange<TDerived>();
            change.Entity = Entity as TDerived;
            change.State = State;
            change.FieldChanges = FieldChanges;
            return change;
        }
    }
}
