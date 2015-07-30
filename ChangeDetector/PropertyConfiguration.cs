using System;
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
            change.FieldName = DisplayName;
            change.OldValue = formatValue(original);
            change.NewValue = formatValue(updated);
            return change;
        }

        private TProp getValue(TEntity entity)
        {
            return Object.Equals(entity, null) ? default(TProp) : Accessor(entity);
        }

        private string formatValue(TEntity entity)
        {
            return Object.Equals(entity, null) ? null : Formatter(Accessor(entity));
        }
    }
}
