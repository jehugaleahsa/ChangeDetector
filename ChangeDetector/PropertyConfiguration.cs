using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal class PropertyConfiguration<TEntity, TProp> : IPropertyConfiguration<TEntity>
        where TEntity : class
    {
        public PropertyConfiguration(PropertyInfo propertyInfo, string displayName, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer)
        {
            Property = propertyInfo;
            DisplayName = displayName;
            Formatter = formatter;
            Comparer = comparer;
        }

        public string DisplayName { get; private set; }

        public PropertyInfo Property { get; private set; }

        public Func<TProp, string> Formatter { get; private set; }

        public IEqualityComparer<TProp> Comparer { get; private set; }

        public IPropertyConfiguration<TBase> GetBaseConfiguration<TBase>() 
            where TBase : class
        {
            return new PropertyConfiguration<TBase, TProp>(Property, DisplayName, Formatter, Comparer);
        }

        public bool IsValueSource(TEntity entity)
        {
            return entity != null && Property.DeclaringType.IsAssignableFrom(entity.GetType());
        }

        public object GetValue(TEntity entity)
        {
            return Property.GetValue(entity);
        }

        public IFieldChange GetChange(TEntity original, TEntity updated)
        {
            var originalSnapshot = getSingletonSnapshot(original);
            var updatedSnapshot = getSingletonSnapshot(updated);
            return GetChange(originalSnapshot, updatedSnapshot);
        }

        private Snapshot getSingletonSnapshot(TEntity entity)
        {
            if (entity == null)
            {
                return Snapshot.Null;
            }
            var snapshot = new Snapshot();
            if (IsValueSource(entity))
            {
                snapshot.Add(Property, GetValue(entity));
            }
            return snapshot;
        }

        public IFieldChange GetChange(Snapshot original, Snapshot updated)
        {
            SnapshotValue originalValue = original.GetValue(Property);
            SnapshotValue updatedValue = updated.GetValue(Property);

            // If both values evaluate to null, then there are no changes. Primitives will never evaluate to null.
            if (originalValue.IsNull() && updatedValue.IsNull())
            {
                return null;
            }

            // If only one of the values evaluates to null, there must have been a change.
            // If both values have values, we need to compare them to see if they changed.
            if (originalValue.IsNull() != updatedValue.IsNull() || !Comparer.Equals(originalValue.GetValue<TProp>(), updatedValue.GetValue<TProp>()))
            {
                return new FieldChange<TEntity, TProp>(this, originalValue, updatedValue);
            }

            return null;
        }
    }
}
