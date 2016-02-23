using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    public class EntityConfiguration<TEntity> : IEntityChangeDetector<TEntity>
        where TEntity : class
    {
        private readonly Dictionary<PropertyInfo, IPropertyConfiguration<TEntity>> properties;
        private readonly Dictionary<Type, IRelatedEntity> relationships;

        public EntityConfiguration()
        {
            properties = new Dictionary<PropertyInfo, IPropertyConfiguration<TEntity>>();
            relationships = new Dictionary<Type, IRelatedEntity>();
        }

        protected internal EntityConfiguration<TEntity> Add<TProp>(string displayName, Expression<Func<TEntity, TProp>> accessor, Func<TProp, string> formatter)
        {
            if (String.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("The property display name cannot be blank.", "displayName");
            }
            if (accessor == null)
            {
                throw new ArgumentNullException("accessor", "The property accessor cannot be null.");
            }
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter", "The property formatter cannot be null.");
            }
            var propertyInfo = GetProperty(accessor);
            var property = new PropertyConfiguration<TEntity, TProp>(displayName, propertyInfo, accessor, formatter);
            properties[propertyInfo] = property;
            return this;
        }

        protected IDerivedEntityConfiguration<TDerived> When<TDerived>()
            where TDerived : class, TEntity
        {
            var detector = new DerivedEntityConfiguration<TEntity, TDerived>(this);
            Func<TEntity, TDerived> accessor = (TEntity e) => e as TDerived;
            IRelatedEntity relationship = new RelatedEntity<TDerived>(detector, accessor);
            relationships[typeof(TDerived)] = relationship;
            return detector;
        }

        public IDerivedEntityChangeDetector<TEntity, TDerived> As<TDerived>()
            where TDerived : class, TEntity
        {
            Func<TEntity, TDerived> accessor = (TEntity e) => e as TDerived;
            IRelatedEntity relationship;
            if (!relationships.TryGetValue(typeof(TDerived), out relationship))
            {
                return new NullDerivedChangeDetector<TEntity, TDerived>();
            }
            RelatedEntity<TDerived> entity = relationship as RelatedEntity<TDerived>;
            if (entity == null)
            {
                return new NullDerivedChangeDetector<TEntity, TDerived>();
            }
            return entity.Detector;
        }

        public IEnumerable<FieldChange> GetChanges(TEntity original, TEntity updated)
        {
            if (original == null && updated == null)
            {
                return new FieldChange[0];
            }

            var propertyChanges = from property in properties.Values
                                  let change = property.GetChange(original, updated)
                                  where change != null
                                  select change;
            var relatedChanges = from relationship in relationships.Values
                                 from change in relationship.GetChanges(original, updated)
                                 select change;
            return propertyChanges.Concat(relatedChanges).ToArray();
        }

        internal IEnumerable<FieldChange> GetChanges(Dictionary<PropertyInfo, object> original, Dictionary<PropertyInfo, object> updated)
        {
            var propertyChanges = from property in properties.Values.Cast<IPropertyExtractor<TEntity>>()
                                  let change = property.GetChange(original, updated)
                                  where change != null
                                  select change;
            var relatedChanges = from relationship in relationships.Values
                                 from change in relationship.GetChanges(original, updated)
                                 select change;
            return propertyChanges.Concat(relatedChanges).ToArray();
        }

        public bool HasChange<TProp>(TEntity original, TEntity updated, Expression<Func<TEntity, TProp>> accessor)
        {
            PropertyInfo propertyInfo = GetProperty<TProp>(accessor);
            return HasChange(original, updated, propertyInfo);
        }

        internal bool HasChange(TEntity original, TEntity updated, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || !properties.ContainsKey(propertyInfo))
            {
                return false;
            }
            var propertyDetector = properties[propertyInfo];
            FieldChange change = propertyDetector.GetChange(original, updated);
            return change != null;
        }

        internal static PropertyInfo GetProperty<TProp>(Expression<Func<TEntity, TProp>> accessor)
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

        private interface IRelatedEntity
        {
            IEnumerable<FieldChange> GetChanges(TEntity original, TEntity updated);

            IEnumerable<FieldChange> GetChanges(Dictionary<PropertyInfo, object> original, Dictionary<PropertyInfo, object> updated);

            IEnumerable<IPropertyExtractor<TEntity>> GetPropertyConfigurations();
        }

        private class RelatedEntity<TRelation> : IRelatedEntity
            where TRelation : class, TEntity
        {
            private readonly Func<TEntity, TRelation> accessor;

            public RelatedEntity(DerivedEntityConfiguration<TEntity, TRelation> detector, Func<TEntity, TRelation> accessor)
            {
                this.Detector = detector;
                this.accessor = accessor;
            }

            public DerivedEntityConfiguration<TEntity, TRelation> Detector { get; private set; }

            public IEnumerable<FieldChange> GetChanges(TEntity original, TEntity updated)
            {
                TRelation originalRelation = getRelation(original);
                TRelation updatedRelation = getRelation(updated);
                return Detector.GetDerivedChanges(originalRelation, updatedRelation);
            }

            private TRelation getRelation(TEntity original)
            {
                return Object.Equals(original, null) ? null : this.accessor(original);
            }

            public IEnumerable<FieldChange> GetChanges(Dictionary<PropertyInfo, object> original, Dictionary<PropertyInfo, object> updated)
            {
                return Detector.GetDerivedChanges(original, updated);
            }

            public IEnumerable<IPropertyExtractor<TEntity>> GetPropertyConfigurations()
            {
                return Detector.GetPropertyConfigurations();
            }
        }

        internal IEnumerable<IPropertyExtractor<TEntity>> GetPropertyConfigurations()
        {
            var propertyConfigurations = properties.Values.Cast<IPropertyExtractor<TEntity>>();
            var relatedConfigurations = from relationship in relationships.Values
                                        from configuration in relationship.GetPropertyConfigurations()
                                        select configuration;
            return propertyConfigurations.Concat(relatedConfigurations).ToArray();
        }
    }
}
