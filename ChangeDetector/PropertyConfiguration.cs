using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal interface IPropertyConfiguration
    {
        PropertyInfo Property { get; }

        Func<object, string> DisplayName { get; }

        Snapshot TakeSingletonSnapshot(object entity);

        IPropertyChange GetChange(Snapshot original, Snapshot updated);

        bool IsValueSource(object entity);

        object GetValue(object entity);
    }

    internal abstract class PropertyConfiguration : IPropertyConfiguration
    {
        public PropertyConfiguration(PropertyInfo propertyInfo, Func<object, string> displayName)
        {
            Property = propertyInfo;
            DisplayName = displayName;
        }

        public PropertyInfo Property { get; private set; }

        public Func<object, string> DisplayName { get; private set; }

        public Snapshot TakeSingletonSnapshot(object entity)
        {
            if (entity == null)
            {
                return Snapshot.Null;
            }
            var snapshot = new Snapshot(entity);
            if (IsValueSource(entity))
            {
                snapshot.Add(Property, GetValue(entity));
            }
            return snapshot;
        }

        public bool IsValueSource(object entity)
        {
            return entity != null && Property.DeclaringType.GetTypeInfo().IsAssignableFrom(entity.GetType().GetTypeInfo());
        }

        public object GetValue(object entity)
        {
            return Property.GetValue(entity);
        }

        public abstract IPropertyChange GetChange(Snapshot original, Snapshot updated);
    }

    internal class PropertyConfiguration<TProp> : PropertyConfiguration
    {
        public PropertyConfiguration(PropertyInfo propertyInfo, Func<object, string> displayName, Func<object, TProp, string> formatter, IEqualityComparer<TProp> comparer)
            : base(propertyInfo, displayName)
        {
            Formatter = formatter;
            Comparer = comparer;
        }

        public Func<object, TProp, string> Formatter { get; private set; }

        public IEqualityComparer<TProp> Comparer { get; private set; }

        public override IPropertyChange GetChange(Snapshot original, Snapshot updated)
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
                return new PropertyChange<TProp>(this, originalValue, updatedValue);
            }

            return null;
        }
    }
}
