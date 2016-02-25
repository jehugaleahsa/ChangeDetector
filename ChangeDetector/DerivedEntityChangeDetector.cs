using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ChangeDetector
{
    public interface IDerivedEntityChangeDetector<TBase, TDerived> : IEntityChangeDetector<TDerived>
        where TBase : class
        where TDerived : class, TBase
    {
        bool HasChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TBase original, TBase updated);

        IFieldChange GetChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TBase original, TBase updated);
    }

    internal class DerivedEntityChangeDetector<TBase, TDerived> : IDerivedEntityChangeDetector<TBase, TDerived>
        where TBase : class
        where TDerived : class, TBase
    {
        private readonly ChangeDetector detector;

        public DerivedEntityChangeDetector(ChangeDetector detector)
        {
            this.detector = detector;
        }

        public IDerivedEntityChangeDetector<TDerived, TSuccessor> As<TSuccessor>() 
            where TSuccessor : class, TDerived
        {
            return new DerivedEntityChangeDetector<TDerived, TSuccessor>(detector);
        }

        public IEnumerable<IFieldChange> GetChanges(TDerived original, TDerived updated)
        {
            Snapshot originalSnapshot = detector.TakeSnapshot(original);
            Snapshot updatedSnapshot = detector.TakeSnapshot(updated);
            return detector.GetChanges(originalSnapshot, updatedSnapshot);
        }

        public bool HasChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TBase original, TBase updated)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            Snapshot originalSnapshot = detector.TakeSnapshot(original, property);
            Snapshot updatedSnapshot = detector.TakeSnapshot(updated, property);
            return detector.HasChange(property, originalSnapshot, updatedSnapshot);
        }

        public bool HasChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TDerived original, TDerived updated)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            Snapshot originalSnapshot = detector.TakeSnapshot(original, property);
            Snapshot updatedSnapshot = detector.TakeSnapshot(updated, property);
            return detector.HasChange(property, originalSnapshot, updatedSnapshot);
        }

        public IFieldChange GetChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TBase original, TBase updated)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            Snapshot originalSnapshot = detector.TakeSnapshot(original, property);
            Snapshot updatedSnapshot = detector.TakeSnapshot(updated, property);
            return detector.GetChange(property, originalSnapshot, updatedSnapshot);
        }

        public IFieldChange GetChange<TProp>(Expression<Func<TDerived, TProp>> accessor, TDerived original, TDerived updated)
        {
            PropertyInfo property = ChangeDetector.GetProperty(accessor);
            Snapshot originalSnapshot = detector.TakeSnapshot(original, property);
            Snapshot updatedSnapshot = detector.TakeSnapshot(updated, property);
            return detector.GetChange(property, originalSnapshot, updatedSnapshot);
        }
    }

}
