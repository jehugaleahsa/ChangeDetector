using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    internal interface IPropertyExtractor<in TEntity>
        where TEntity : class
    {
        PropertyInfo Property { get; }

        object GetValue(TEntity entity);

        FieldChange GetChange(Dictionary<PropertyInfo, object> original, Dictionary<PropertyInfo, object> updated);

        bool IsValueSource(TEntity entity);
    }

    internal class PropertyConfiguration<TEntity, TProp> : IPropertyConfiguration<TEntity>, IPropertyExtractor<TEntity>
        where TEntity : class
    {
        public PropertyConfiguration(
            string displayName, 
            PropertyInfo propertyInfo,
            Expression<Func<TEntity, TProp>> accessor, 
            Func<TProp, string> formatter)
        {
            DisplayName = displayName;
            Property = propertyInfo;
            Accessor = accessor.Compile();
            Formatter = formatter;
        }

        public string DisplayName { get; private set; }

        public Func<TEntity, TProp> Accessor { get; private set; }

        public PropertyInfo Property { get; private set; }

        public Func<TProp, string> Formatter { get; private set; }

        public FieldChange GetChange(TEntity original, TEntity updated)
        {
            TProp firstValue = getValue(original);
            TProp secondValue = getValue(updated);
            if (Object.Equals(firstValue, secondValue))
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

        bool IPropertyExtractor<TEntity>.IsValueSource(TEntity entity)
        {
            return entity.GetType().IsAssignableFrom(Property.DeclaringType);
        }

        object IPropertyExtractor<TEntity>.GetValue(TEntity entity)
        {
            return getValue(entity);
        }

        private TProp getValue(TEntity entity)
        {
            return Object.Equals(entity, null) ? default(TProp) : Accessor(entity);
        }

        private string formatValue(TEntity entity)
        {
            return Object.Equals(entity, null) ? null : Formatter(Accessor(entity));
        }

        FieldChange IPropertyExtractor<TEntity>.GetChange(Dictionary<PropertyInfo, object> original, Dictionary<PropertyInfo, object> updated)
        {
            TProp firstValue = getValue(original);
            TProp secondValue = getValue(updated);
            if (Object.Equals(firstValue, secondValue))
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
