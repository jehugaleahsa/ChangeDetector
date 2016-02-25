using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ChangeDetector
{
    public interface IEntityChangeDetector<TEntity>
        where TEntity : class
    {
        IDerivedEntityChangeDetector<TEntity, TDerived> As<TDerived>() where TDerived : class, TEntity;

        IPropertyChange GetChange<TProp>(Expression<Func<TEntity, TProp>> accessor, TEntity original, TEntity updated);

        IEnumerable<IPropertyChange> GetChanges(TEntity original, TEntity updated);

        bool HasChange<TProp>(Expression<Func<TEntity, TProp>> accessor, TEntity original, TEntity updated);
    }
}
