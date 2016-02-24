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

        public IDerivedEntityConfiguration<TDerived> Add<TProp>(string displayName, Expression<Func<TDerived, TProp>> accessor, Func<TProp, string> formatter)
        {
            derivedConfiguration.Add(displayName, accessor, formatter);
            return this;
        }

        public IDerivedEntityChangeDetector<TDerived, TSuccessor> As<TSuccessor>() where TSuccessor : class, TDerived
        {
            return derivedConfiguration.As<TSuccessor>();
        }

        public IEnumerable<FieldChange> GetChanges(TDerived original, TDerived updated)
        {
            return baseConfiguration.GetChanges(original, updated).Concat(derivedConfiguration.GetChanges(original, updated)).ToArray();
        }

        public IEnumerable<FieldChange> GetDerivedChanges(Dictionary<PropertyInfo, object> original, Dictionary<PropertyInfo, object> updated)
        {
            return derivedConfiguration.GetChanges(original, updated);
        }

        public bool HasChange<TProp>(TDerived original, TDerived updated, Expression<Func<TDerived, TProp>> accessor)
        {
            PropertyInfo propertyInfo = EntityConfiguration<TDerived>.GetProperty(accessor);
            return baseConfiguration.HasChange(original, updated, propertyInfo) || derivedConfiguration.HasChange(original, updated, propertyInfo);
        }

        public bool HasChange<TProp>(TBase original, TBase updated, Expression<Func<TDerived, TProp>> accessor)
        {
            // If the property is in the base object, don't bother casting.
            PropertyInfo propertyInfo = EntityConfiguration<TDerived>.GetProperty(accessor);
            if (baseConfiguration.HasChange(original, updated, propertyInfo))
            {
                return true;
            }
            // If either of the objects are not TDerived, we are comparing unrelated objects.
            // For now we will just return false instead of throwing an exception.
            if (!(original is TDerived) || !(updated is TDerived))
            {
                return false;
            }
            return derivedConfiguration.HasChange((TDerived)original, (TDerived)updated, propertyInfo);
        }

        internal IEnumerable<IPropertyConfiguration<TBase>> GetPropertyConfigurations()
        {
            return derivedConfiguration.GetPropertyConfigurations()
                .Select(c => c.GetBaseConfiguration<TBase>())
                .ToArray();
        }
    }
}
