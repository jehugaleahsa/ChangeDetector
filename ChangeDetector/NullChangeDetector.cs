using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ChangeDetector
{
    internal class NullChangeDetector<TEntity> : IEntityChangeDetector<TEntity>
        where TEntity : class
    {
        public IEnumerable<IFieldChange> GetChanges(TEntity original, TEntity updated)
        {
            return new IFieldChange[0];
        }

        public bool HasChange<TProp>(Expression<Func<TEntity, TProp>> accessor, TEntity original, TEntity updated)
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
        public bool HasChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TBase original, TBase updated)
        {
            return false;
        }

        public IDerivedEntityChangeDetector<TDerived, TSuccessor> As<TSuccessor>() where TSuccessor : class, TDerived
        {
            return new NullDerivedChangeDetector<TDerived, TSuccessor>();
        }

        public IEnumerable<IFieldChange> GetChanges(TDerived original, TDerived updated)
        {
            return new IFieldChange[0];
        }

        public bool HasChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TDerived original, TDerived updated)
        {
            return false;
        }
    }
}
