using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    public class EntityConfiguration<TEntity> : IEntityChangeDetector<TEntity>, IDerivedEntityConfiguration<TEntity>
        where TEntity : class
    {
        private readonly ChangeDetector detector;

        public EntityConfiguration()
        {
            this.detector = new ChangeDetector();
        }

        private EntityConfiguration(ChangeDetector detector)
        {
            this.detector = detector;
        }

        protected EntityConfiguration<TEntity> Add<TProp>(Expression<Func<TEntity, TProp>> accessor, IEqualityComparer<TProp> comparer = null)
        {
            var propertyInfo = ChangeDetector.GetProperty<TEntity, TProp>(accessor);
            detector.Add(propertyInfo, null, null, comparer);
            return this;
        }

        protected EntityConfiguration<TEntity> Add<TProp>(Expression<Func<TEntity, TProp>> accessor, string displayName, IEqualityComparer<TProp> comparer = null)
        {
            var propertyInfo = ChangeDetector.GetProperty<TEntity, TProp>(accessor);
            detector.Add(propertyInfo, displayName, null, comparer);
            return this;
        }

        protected EntityConfiguration<TEntity> Add<TProp>(Expression<Func<TEntity, TProp>> accessor, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer = null)
        {
            var propertyInfo = ChangeDetector.GetProperty<TEntity, TProp>(accessor);
            detector.Add(propertyInfo, null, formatter, comparer);
            return this;
        }

        protected EntityConfiguration<TEntity> Add<TProp>(Expression<Func<TEntity, TProp>> accessor, string displayName, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer = null)
        {
            var propertyInfo = ChangeDetector.GetProperty<TEntity, TProp>(accessor);
            detector.Add(propertyInfo, displayName, formatter, comparer);
            return this;
        }

        protected EntityConfiguration<TEntity> AddCollection<TElement>(Expression<Func<TEntity, ICollection<TElement>>> accessor, IEqualityComparer<TElement> comparer = null)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            detector.AddCollection(property, null, comparer);
            return this;
        }

        protected EntityConfiguration<TEntity> AddCollection<TElement>(Expression<Func<TEntity, ICollection<TElement>>> accessor, string displayName, IEqualityComparer<TElement> comparer = null)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            detector.AddCollection(property, displayName, comparer);
            return this;
        }

        protected IDerivedEntityConfiguration<TDerived> When<TDerived>()
            where TDerived : class, TEntity
        {
            return new EntityConfiguration<TDerived>(detector);
        }

        public IDerivedEntityChangeDetector<TEntity, TDerived> As<TDerived>()
            where TDerived : class, TEntity
        {
            return new DerivedEntityChangeDetector<TEntity, TDerived>(detector);
        }

        public IEnumerable<IPropertyChange> GetChanges(TEntity original, TEntity updated)
        {
            var originalSnapshot = detector.TakeSnapshot(original);
            var updatedSnapshot = detector.TakeSnapshot(updated);
            return detector.GetChanges(originalSnapshot, updatedSnapshot);
        }

        public IPropertyChange GetChange<TProp>(Expression<Func<TEntity, TProp>> accessor, TEntity original, TEntity updated)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            Snapshot originalSnapshot = detector.TakeSnapshot(original, property);
            Snapshot updatedSnapshot = detector.TakeSnapshot(updated, property);
            return detector.GetChange(property, originalSnapshot, updatedSnapshot);
        }

        public bool HasChange<TProp>(Expression<Func<TEntity, TProp>> accessor, TEntity original, TEntity updated)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            Snapshot originalSnapshot = detector.TakeSnapshot(original, property);
            Snapshot updatedSnapshot = detector.TakeSnapshot(updated, property);
            return detector.HasChange(property, originalSnapshot, updatedSnapshot);
        }

        IDerivedEntityConfiguration<TEntity> IDerivedEntityConfiguration<TEntity>.Add<TProp>(Expression<Func<TEntity, TProp>> accessor, IEqualityComparer<TProp> comparer)
        {
            return Add(accessor, comparer);
        }

        IDerivedEntityConfiguration<TEntity> IDerivedEntityConfiguration<TEntity>.Add<TProp>(Expression<Func<TEntity, TProp>> accessor, string displayName, IEqualityComparer<TProp> comparer)
        {
            return Add(accessor, displayName, comparer);
        }

        IDerivedEntityConfiguration<TEntity> IDerivedEntityConfiguration<TEntity>.Add<TProp>(Expression<Func<TEntity, TProp>> accessor, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer)
        {
            return Add(accessor, formatter, comparer);
        }

        IDerivedEntityConfiguration<TEntity> IDerivedEntityConfiguration<TEntity>.Add<TProp>(Expression<Func<TEntity, TProp>> accessor, string displayName, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer)
        {
            return Add(accessor, displayName, formatter, comparer);
        }

        IDerivedEntityConfiguration<TEntity> IDerivedEntityConfiguration<TEntity>.AddCollection<TElement>(Expression<Func<TEntity, ICollection<TElement>>> accessor, IEqualityComparer<TElement> comparer)
        {
            return AddCollection(accessor, comparer);
        }

        IDerivedEntityConfiguration<TEntity> IDerivedEntityConfiguration<TEntity>.AddCollection<TElement>(Expression<Func<TEntity, ICollection<TElement>>> accessor, string displayName, IEqualityComparer<TElement> comparer)
        {
            return AddCollection(accessor, displayName, comparer);
        }

        internal Snapshot TakeSnapshot(object entity)
        {
            return detector.TakeSnapshot(entity);
        }

        internal IEnumerable<IPropertyChange> GetChanges(Snapshot original, Snapshot updated)
        {
            return detector.GetChanges(original, updated);
        }

        internal CollectionSnapshotLookup TakeCollectionSnapshots(object entity)
        {
            return detector.TakeCollectionSnapshots(entity);
        }

        internal ICollectionChange<TElement> GetCollectionChanges<TElement>(Expression<Func<TEntity, ICollection<TElement>>> accessor, CollectionSnapshotLookup original, TEntity updated, ElementState state)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            return detector.GetCollectionChanges<TElement>(property, original, updated, state);
        }
    }
}
