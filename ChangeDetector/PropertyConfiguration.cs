using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        private Expression<Func<TEntity, TProp>> Accessor { get; set; }

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
            return Property.DeclaringType.IsAssignableFrom(entity.GetType());
        }

        public object GetValue(TEntity entity)
        {
            return Property.GetValue(entity);
        }

        public FieldChange GetChange(TEntity original, TEntity updated)
        {
            Dictionary<PropertyInfo, object> originalSnapshot = getValue(original);
            Dictionary<PropertyInfo, object> updatedSnapshot = getValue(updated);
            return GetChange(originalSnapshot, updatedSnapshot);
        }

        private Dictionary<PropertyInfo, object> getValue(TEntity entity)
        {
            var snapshot = new Dictionary<PropertyInfo, object>();
            if (entity != null)
            {
                snapshot.Add(Property, (TProp)Property.GetValue(entity));
            }
            return snapshot;
        }

        public FieldChange GetChange(Dictionary<PropertyInfo, object> original, Dictionary<PropertyInfo, object> updated)
        {
            TProp firstValue = getValue(original);
            TProp secondValue = getValue(updated);
            if (Comparer.Equals(firstValue, secondValue))
            {
                return null;
            }

            FieldChange change = new FieldChange();
            change.Property = Property;
            change.FieldName = DisplayName;
            change.OldValue = formatValue(original);
            change.NewValue = formatValue(updated);
            return change;
        }

        private TProp getValue(Dictionary<PropertyInfo, object> entity)
        {
            return entity.ContainsKey(Property) ? (TProp)entity[Property] : default(TProp);
        }

        private string formatValue(Dictionary<PropertyInfo, object> entity)
        {
            return entity.ContainsKey(Property) ? Formatter((TProp)entity[Property]) : null;
        }
    }
}
