using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ChangeDetector
{
    public interface IEntityChangeDetector<TEntity>
        where TEntity : class
    {
        IEntityChangeDetector<TDerived> As<TDerived>() where TDerived : class, TEntity;

        IEnumerable<FieldChange> GetChanges(TEntity original, TEntity updated);

        bool HasChange<TProp>(TEntity original, TEntity updated, Expression<Func<TEntity, TProp>> accessor);
    }
}
