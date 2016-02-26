using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ChangeDetector
{
    public interface IDerivedEntityConfiguration<TDerived>
        where TDerived : class
    {
        IDerivedEntityConfiguration<TDerived> Add<TProp>(Expression<Func<TDerived, TProp>> accessor, IEqualityComparer<TProp> comparer = null);

        IDerivedEntityConfiguration<TDerived> Add<TProp>(Expression<Func<TDerived, TProp>> accessor, string displayName, IEqualityComparer<TProp> comparer = null);

        IDerivedEntityConfiguration<TDerived> Add<TProp>(Expression<Func<TDerived, TProp>> accessor, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer = null);

        IDerivedEntityConfiguration<TDerived> Add<TProp>(Expression<Func<TDerived, TProp>> accessor, string displayName, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer = null);

        IDerivedEntityConfiguration<TDerived> AddCollection<TElement>(Expression<Func<TDerived, ICollection<TElement>>> accessor, IEqualityComparer<TElement> comparer = null);

        IDerivedEntityConfiguration<TDerived> AddCollection<TElement>(Expression<Func<TDerived, ICollection<TElement>>> accessor, string displayName, IEqualityComparer<TElement> comparer = null);
    }
}
