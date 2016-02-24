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

        FieldChange GetChange(TEntity original, TEntity updated);

        FieldChange GetChange(Dictionary<PropertyInfo, object> original, Dictionary<PropertyInfo, object> updated);

        bool IsValueSource(TEntity entity);

        object GetValue(TEntity entity);

        IPropertyConfiguration<TBase> GetBaseConfiguration<TBase>() where TBase : class;
    }
}
