using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    public class EntityConfiguration<TEntity> : IEntityChangeDetector<TEntity>, IDerivedEntityConfiguration<TEntity>
        where TEntity : class
    {
        private readonly Dictionary<PropertyInfo, IPropertyConfiguration<TEntity>> properties;
        private readonly Dictionary<object, IRelatedEntity> relationships;

        public EntityConfiguration()
        {
            properties = new Dictionary<PropertyInfo, IPropertyConfiguration<TEntity>>();
            relationships = new Dictionary<object, IRelatedEntity>();
        }

        protected EntityConfiguration<TEntity> Add<TProp>(string displayName, Expression<Func<TEntity, TProp>> accessor, Func<TProp, string> formatter)
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
            var propertyInfo = getProperty(accessor);
            var property = new PropertyConfiguration<TEntity, TProp>(displayName, propertyInfo, accessor, formatter);
            properties[propertyInfo] = property;
            return this;
        }

        IDerivedEntityConfiguration<TEntity> IDerivedEntityConfiguration<TEntity>.Add<TProp>(string displayName, Expression<Func<TEntity, TProp>> accessor, Func<TProp, string> formatter)
        {
            return Add(displayName, accessor, formatter);
        }

        protected IDerivedEntityConfiguration<TDerived> When<TDerived>()
            where TDerived : class, TEntity
        {
            EntityConfiguration<TDerived> detector = new EntityConfiguration<TDerived>();
            Func<TEntity, TDerived> accessor = (TEntity e) => e as TDerived;
            IRelatedEntity relationship = new RelatedEntity<TDerived>(detector, accessor);
            relationships[accessor] = relationship;
            return detector;
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

        public bool HasChange<TProp>(TEntity original, TEntity updated, Expression<Func<TEntity, TProp>> accessor)
        {
            PropertyInfo propertyInfo = getProperty<TProp>(accessor);
            if (!properties.ContainsKey(propertyInfo))
            {
                throw new ArgumentException("The accessor refers to an unconfigured property.", "accessor");
            }
            var propertyDetector = properties[propertyInfo];
            FieldChange change = propertyDetector.GetChange(original, updated);
            return change != null;
        }

        private static PropertyInfo getProperty<TProp>(Expression<Func<TEntity, TProp>> accessor)
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
        }

        private class RelatedEntity<TRelation> : IRelatedEntity
            where TRelation : class
        {
            private readonly IEntityChangeDetector<TRelation> detector;
            private readonly Func<TEntity, TRelation> accessor;

            public RelatedEntity(IEntityChangeDetector<TRelation> detector, Func<TEntity, TRelation> accessor)
            {
                this.detector = detector;
                this.accessor = accessor;
            }

            public IEnumerable<FieldChange> GetChanges(TEntity original, TEntity updated)
            {
                TRelation originalRelation = getRelation(original);
                TRelation updatedRelation = getRelation(updated);
                return detector.GetChanges(originalRelation, updatedRelation);
            }

            private TRelation getRelation(TEntity original)
            {
                return Object.Equals(original, null) ? null : this.accessor(original);
            }
        }
    }
}
