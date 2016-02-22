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

        public IDerivedEntityChangeDetector<TEntity, TDerived> As<TDerived>() where TDerived : class, TEntity
        {
            return new NullDerivedChangeDetector<TEntity, TDerived>();
        }
    }

    internal class NullDerivedChangeDetector<TBase, TDerived> : IDerivedEntityChangeDetector<TBase, TDerived>
        where TBase : class
        where TDerived : class, TBase
    {
        public bool HasChange<TProp>(TBase original, TBase updated, Expression<Func<TDerived, TProp>> accessor)
        {
            return false;
        }

        public IDerivedEntityChangeDetector<TDerived, TSuccessor> As<TSuccessor>() where TSuccessor : class, TDerived
        {
            return new NullDerivedChangeDetector<TDerived, TSuccessor>();
        }

        public IEnumerable<FieldChange> GetChanges(TDerived original, TDerived updated)
        {
            return new FieldChange[0];
        }

        public bool HasChange<TProp>(TDerived original, TDerived updated, Expression<Func<TDerived, TProp>> accessor)
        {
            return false;
        }
    }
}
