using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    internal class DerivedEntityConfiguration<TBase, TDerived> : IDerivedEntityConfiguration<TDerived>, IDerivedEntityChangeDetector<TBase, TDerived>
        where TBase : class
        where TDerived : class, TBase
    {
        private readonly EntityConfiguration<TBase> baseConfiguration;
        private readonly EntityConfiguration<TDerived> derivedConfiguration;

        public DerivedEntityConfiguration(EntityConfiguration<TBase> parent)
        {
            this.baseConfiguration = parent;
            this.derivedConfiguration = new EntityConfiguration<TDerived>();
        }

        public IDerivedEntityConfiguration<TDerived> Add<TProp>(Expression<Func<TDerived, TProp>> accessor, IEqualityComparer<TProp> comparer = null)
        {
            derivedConfiguration.Add(accessor, comparer);
            return this;
        }

        public IDerivedEntityConfiguration<TDerived> Add<TProp>(Expression<Func<TDerived, TProp>> accessor, string displayName, IEqualityComparer<TProp> comparer = null)
        {
            derivedConfiguration.Add(accessor, displayName, comparer);
            return this;
        }

        public IDerivedEntityConfiguration<TDerived> Add<TProp>(Expression<Func<TDerived, TProp>> accessor, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer = null)
        {
            derivedConfiguration.Add(accessor, formatter, comparer);
            return this;
        }

        public IDerivedEntityConfiguration<TDerived> Add<TProp>(Expression<Func<TDerived, TProp>> accessor, string displayName, Func<TProp, string> formatter, IEqualityComparer<TProp> comparer = null)
        {
            derivedConfiguration.Add(accessor, displayName, formatter, comparer);
            return this;
        }

        public IDerivedEntityChangeDetector<TDerived, TSuccessor> As<TSuccessor>() where TSuccessor : class, TDerived
        {
            return derivedConfiguration.As<TSuccessor>();
        }

        public IEnumerable<IFieldChange> GetChanges(TDerived original, TDerived updated)
        {
            return baseConfiguration.GetChanges(original, updated).Concat(derivedConfiguration.GetChanges(original, updated)).ToArray();
        }

        public IEnumerable<IFieldChange> GetDerivedChanges(Snapshot original, Snapshot updated)
        {
            return derivedConfiguration.GetChanges(original, updated);
        }

        public bool HasChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TDerived original, TDerived updated)
        {
            PropertyInfo propertyInfo = EntityConfiguration<TDerived>.GetProperty(accessor);
            return baseConfiguration.HasChange(propertyInfo, original, updated) || derivedConfiguration.HasChange(propertyInfo, original, updated);
        }

        public bool HasChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TBase original, TBase updated)
        {
            // If the property is in the base object, don't bother casting.
            PropertyInfo propertyInfo = EntityConfiguration<TDerived>.GetProperty(accessor);
            if (baseConfiguration.HasChange(propertyInfo, original, updated))
            {
                return true;
            }
            // If either of the objects are not TDerived, we are comparing unrelated objects.
            // For now we will just return false instead of throwing an exception.
            if (!(original is TDerived) || !(updated is TDerived))
            {
                return false;
            }
            return derivedConfiguration.HasChange(propertyInfo, (TDerived)original, (TDerived)updated);
        }

        internal IEnumerable<IPropertyConfiguration<TBase>> GetPropertyConfigurations()
        {
            return derivedConfiguration.GetPropertyConfigurations()
                .Select(c => c.GetBaseConfiguration<TBase>())
                .ToArray();
        }
    }
}
