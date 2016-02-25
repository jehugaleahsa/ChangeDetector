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

        public IEnumerable<IFieldChange> GetChanges(TEntity original, TEntity updated)
        {
            var originalSnapshot = detector.TakeSnapshot(original);
            var updatedSnapshot = detector.TakeSnapshot(updated);
            return detector.GetChanges(originalSnapshot, updatedSnapshot);
        }

        public bool HasChange<TProp>(Expression<Func<TEntity, TProp>> accessor, TEntity original, TEntity updated)
        {
            PropertyInfo propertyInfo = ChangeDetector.GetProperty(accessor);
            Snapshot originalSnapshot = detector.TakeSnapshot(original);
            Snapshot updatedSnapshot = detector.TakeSnapshot(updated);
            return detector.HasChange(propertyInfo, originalSnapshot, updatedSnapshot);
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

        internal Snapshot TakeSnapshot(object entity)
        {
            return detector.TakeSnapshot(entity);
        }

        internal IEnumerable<IFieldChange> GetChanges(Snapshot original, Snapshot updated)
        {
            return detector.GetChanges(original, updated);
        }
    }
}
