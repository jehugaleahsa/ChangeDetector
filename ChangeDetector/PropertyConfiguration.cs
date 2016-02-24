using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal class PropertyConfiguration<TEntity, TProp> : IPropertyConfiguration<TEntity>
        where TEntity : class
    {
        public PropertyConfiguration(
            string displayName, 
            PropertyInfo propertyInfo,
            Func<TProp, string> formatter,
            IEqualityComparer<TProp> comparer)
        {
            DisplayName = displayName;
            Property = propertyInfo;
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
            return new PropertyConfiguration<TBase, TProp>(
                DisplayName,
                Property,
                Formatter,
                Comparer);
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
            // If both snapshots represent null entities, then there is not a change.
            if (original.IsNull() && updated.IsNull())
            {
                return null;
            }
            // If the snapshot represents a null entity, we say it has a value for the property.
            // If the property is not in either lookup, it means we're looking at a base class.
            // If the property is only in one lookup, it means we comparing different classes in a hierarchy.
            bool hasOriginal = original.HasValue(Property);
            bool hasUpdated = updated.HasValue(Property);
            if (hasOriginal != hasUpdated)
            {
                return null;
            }

            TProp firstValue = original.GetValue<TProp>(Property);
            TProp secondValue = updated.GetValue<TProp>(Property);
            if (Comparer.Equals(firstValue, secondValue))
            {
                return null;
            }

            IFieldChange change = new FieldChange<TEntity, TProp>(this, firstValue, secondValue);
            return change;
        }
    }
}
