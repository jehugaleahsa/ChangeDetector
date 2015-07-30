using System;
using System.Reflection;

namespace ChangeDetector
{
    public interface IPropertyConfiguration<TEntity>
        where TEntity : class
    {
        string DisplayName { get; }

        PropertyInfo Property { get; }

        FieldChange GetChange(TEntity original, TEntity updated);
    }
}
