using System;
using System.Linq.Expressions;

namespace ChangeDetector
{
    public interface IDerivedEntityChangeDetector<TBase, TDerived> : IEntityChangeDetector<TDerived>
        where TBase : class
        where TDerived : class, TBase
    {
        bool HasChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TBase original, TBase updated);
    }
}
