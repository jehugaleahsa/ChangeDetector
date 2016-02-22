using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ChangeDetector
{
    internal class NullChangeDetector<TEntity> : IEntityChangeDetector<TEntity>
        where TEntity : class
    {
        public IEnumerable<FieldChange> GetChanges(TEntity original, TEntity updated)
        {
            return new FieldChange[0];
        }

        public bool HasChange<TProp>(TEntity original, TEntity updated, Expression<Func<TEntity, TProp>> accessor)
        {
            return false;
        }

        public IEntityChangeDetector<TDerived> As<TDerived>() where TDerived : class, TEntity
        {
            return new NullChangeDetector<TDerived>();
        }
    }
}
