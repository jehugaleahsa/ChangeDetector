using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    internal class ChangeDetector
    {
        private readonly Dictionary<PropertyInfo, IPropertyConfiguration> properties;

        public ChangeDetector()
        {
            this.properties = new Dictionary<PropertyInfo, IPropertyConfiguration>();
        }

        public void Add<TProp>(PropertyInfo propertyInfo, string displayName, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer)
        {
            if (String.IsNullOrWhiteSpace(displayName))
            {
                displayName = propertyInfo.Name;
            }
            if (formatter == null)
            {
                formatter = p => p == null ? null : p.ToString();
            }
            if (comparer == null)
            {
                comparer = EqualityComparer<TProp>.Default;
            }
            var property = new PropertyConfiguration<TProp>(propertyInfo, displayName, formatter, comparer);
            properties[propertyInfo] = property;
        }

        public IEnumerable<IFieldChange> GetChanges(Snapshot original, Snapshot updated)
        {
            var propertyChanges = from property in properties.Values
                                  let change = property.GetChange(original, updated)
                                  where change != null
                                  select change;
            return propertyChanges.ToArray();
        }

        public bool HasChange(PropertyInfo propertyInfo, Snapshot original, Snapshot updated)
        {
            if (propertyInfo == null || !properties.ContainsKey(propertyInfo))
            {
                return false;
            }
            var propertyDetector = properties[propertyInfo];
            IFieldChange change = propertyDetector.GetChange(original, updated);
            return change != null;
        }

        public static PropertyInfo GetProperty<TEntity, TProp>(Expression<Func<TEntity, TProp>> accessor)
        {
            if (accessor == null)
            {
                throw new ArgumentNullException("accessor");
            }
            MemberExpression memberExpression = accessor.Body as MemberExpression;
            if (memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("The accessor does not refer to a property of the entity.", "accessor");
            }
            PropertyInfo propertyInfo = (PropertyInfo)memberExpression.Member;
            return propertyInfo;
        }

        public Snapshot TakeSnapshot(object entity)
        {
            if (entity == null)
            {
                return Snapshot.Null;
            }
            Snapshot snapshot = new Snapshot();
            foreach (var configuration in properties.Values)
            {
                if (configuration.IsValueSource(entity))
                {
                    snapshot.Add(configuration.Property, configuration.GetValue(entity));
                }
            }
            return snapshot;
        }
    }
}
