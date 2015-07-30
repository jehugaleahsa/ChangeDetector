using System;
using System.Linq.Expressions;

namespace ChangeDetector
{
    public interface IDerivedEntityConfiguration<TDerived>
        where TDerived : class
    {
        IDerivedEntityConfiguration<TDerived> Add<TProp>(string displayName, Expression<Func<TDerived, TProp>> accessor, Func<TProp, string> formatter);
    }
}
