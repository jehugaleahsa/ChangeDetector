using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal interface IPropertyConfiguration<TEntity>
        where TEntity : class
    {
        string DisplayName { get; }

        PropertyInfo Property { get; }

        IFieldChange GetChange(TEntity original, TEntity updated);

        IFieldChange GetChange(Snapshot original, Snapshot updated);

        IPropertyConfiguration<TBase> GetBaseConfiguration<TBase>() where TBase : class;

        bool IsValueSource(TEntity entity);

        object GetValue(TEntity entity);
    }
}
