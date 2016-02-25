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

        protected internal EntityConfiguration<TEntity> Add<TProp>(Expression<Func<TEntity, TProp>> accessor, IEqualityComparer<TProp> comparer = null)
        {
            if (accessor == null)
            {
                throw new ArgumentNullException("accessor");
            }
            if (comparer == null)
            {
                comparer = EqualityComparer<TProp>.Default;
            }
            var propertyInfo = GetProperty(accessor);
            Func<TProp, string> formatter = p => p == null ? null : p.ToString();
            return add<TProp>(propertyInfo, propertyInfo.Name, formatter, comparer);
        }

        protected internal EntityConfiguration<TEntity> Add<TProp>(Expression<Func<TEntity, TProp>> accessor, string displayName, IEqualityComparer<TProp> comparer = null)
        {
            if (String.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("The display name cannot be blank.", "displayName");
            }
            if (accessor == null)
            {
                throw new ArgumentNullException("accessor");
            }
            if (comparer == null)
            {
                comparer = EqualityComparer<TProp>.Default;
            }
            var propertyInfo = GetProperty(accessor);
            Func<TProp, string> formatter = p => p == null ? null : p.ToString();
            return add<TProp>(propertyInfo, displayName, formatter, comparer);
        }

        protected internal EntityConfiguration<TEntity> Add<TProp>(Expression<Func<TEntity, TProp>> accessor, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer = null)
        {
            if (accessor == null)
            {
                throw new ArgumentNullException("accessor");
            }
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }
            if (comparer == null)
            {
                comparer = EqualityComparer<TProp>.Default;
            }
            var propertyInfo = GetProperty(accessor);
            return add<TProp>(propertyInfo, propertyInfo.Name, formatter, comparer);
        }

        protected internal EntityConfiguration<TEntity> Add<TProp>(Expression<Func<TEntity, TProp>> accessor, string displayName, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer = null)
        {
            if (accessor == null)
            {
                throw new ArgumentNullException("accessor", "The property accessor cannot be null.");
            }
            if (String.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("The property display name cannot be blank.", "displayName");
            }
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter", "The property formatter cannot be null.");
            }
            if (comparer == null)
            {
                comparer = EqualityComparer<TProp>.Default;
            }
            var propertyInfo = GetProperty<TProp>(accessor);
            return add<TProp>(propertyInfo, displayName, formatter, comparer);
        }

        private EntityConfiguration<TEntity> add<TProp>(PropertyInfo propertyInfo, string displayName, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer)
        {
            var property = new PropertyConfiguration<TEntity, TProp>(displayName, propertyInfo, formatter, comparer);
            properties[propertyInfo] = property;
            return this;
        }

        protected IDerivedEntityConfiguration<TDerived> When<TDerived>()
            where TDerived : class, TEntity
        {
            var detector = new DerivedEntityConfiguration<TEntity, TDerived>(this);
            IRelatedEntity relationship = new RelatedEntity<TDerived>(detector);
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

        public IEnumerable<IFieldChange> GetChanges(TEntity original, TEntity updated)
        {
            var originalSnapshot = TakeSnapshot(original);
            var updatedSnapshot = TakeSnapshot(updated);
            return GetChanges(originalSnapshot, updatedSnapshot);
        }

        internal IEnumerable<IFieldChange> GetChanges(Snapshot original, Snapshot updated)
        {
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
            IFieldChange change = propertyDetector.GetChange(original, updated);
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
            IEnumerable<IFieldChange> GetChanges(Snapshot original, Snapshot updated);

            IEnumerable<IPropertyConfiguration<TEntity>> GetPropertyConfigurations();
        }

        private class RelatedEntity<TRelation> : IRelatedEntity
            where TRelation : class, TEntity
        {
            public RelatedEntity(DerivedEntityConfiguration<TEntity, TRelation> detector)
            {
                this.Detector = detector;
            }

            public DerivedEntityConfiguration<TEntity, TRelation> Detector { get; private set; }

            public IEnumerable<IFieldChange> GetChanges(Snapshot original, Snapshot updated)
            {
                return Detector.GetDerivedChanges(original, updated);
            }

            public IEnumerable<IPropertyConfiguration<TEntity>> GetPropertyConfigurations()
            {
                return Detector.GetPropertyConfigurations();
            }
        }

        internal Snapshot TakeSnapshot(TEntity entity)
        {
            if (entity == null)
            {
                return Snapshot.Null;
            }
            Snapshot snapshot = new Snapshot();
            foreach (var configuration in GetPropertyConfigurations())
            {
                if (configuration.IsValueSource(entity))
                {
                    snapshot.Add(configuration.Property, configuration.GetValue(entity));
                }
            }
            return snapshot;
        }

        internal IEnumerable<IPropertyConfiguration<TEntity>> GetPropertyConfigurations()
        {
            var relatedConfigurations = from relationship in relationships.Values
                                        from configuration in relationship.GetPropertyConfigurations()
                                        select configuration;
            return properties.Values.Concat(relatedConfigurations).ToArray();
        }
    }
}
