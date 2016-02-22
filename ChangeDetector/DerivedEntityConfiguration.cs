using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    internal class DerivedEntityConfiguration<TBase, TDerived> : IDerivedEntityConfiguration<TDerived>, IEntityChangeDetector<TDerived>
        where TBase : class
        where TDerived : class, TBase
    {
        private readonly EntityConfiguration<TBase> parent;
        private readonly EntityConfiguration<TDerived> derived;

        public DerivedEntityConfiguration(EntityConfiguration<TBase> parent)
        {
            this.parent = parent;
            this.derived = new EntityConfiguration<TDerived>();
        }

        public IDerivedEntityConfiguration<TDerived> Add<TProp>(string displayName, Expression<Func<TDerived, TProp>> accessor, Func<TProp, string> formatter)
        {
            derived.Add(displayName, accessor, formatter);
            return this;
        }

        public IEntityChangeDetector<TSuccessor> As<TSuccessor>() where TSuccessor : class, TDerived
        {
            return derived.As<TSuccessor>();
        }

        public IEnumerable<FieldChange> GetChanges(TDerived original, TDerived updated)
        {
            return parent.GetChanges(original, updated).Concat(derived.GetChanges(original, updated)).ToArray();
        }

        public IEnumerable<FieldChange> GetDerivedChanges(TDerived original, TDerived updated)
        {
            return derived.GetChanges(original, updated);
        }

        public bool HasChange<TProp>(TDerived original, TDerived updated, Expression<Func<TDerived, TProp>> accessor)
        {
            PropertyInfo propertyInfo = EntityConfiguration<TDerived>.GetProperty(accessor);
            return parent.HasChange(original, updated, propertyInfo) || derived.HasChange(original, updated, accessor);
        }
    }
}
