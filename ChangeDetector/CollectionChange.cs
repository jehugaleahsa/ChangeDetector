using System;
using System.Reflection;

namespace ChangeDetector
{
    public interface ICollectionChange<TElement>
    {
        PropertyInfo Property { get; }

        string DisplayName { get; }

        ElementChangeCollection<TElement> GetChanges();
    }

    internal class CollectionChange<TElement> : ICollectionChange<TElement>
    {
        private readonly ElementChangeCollection<TElement> changes;

        public CollectionChange(
            PropertyInfo property, 
            string displayName, 
            ElementChangeCollection<TElement> changes)
        {
            this.Property = property;
            this.DisplayName = displayName;
            this.changes = changes;
        }

        public PropertyInfo Property { get; private set; }

        public string DisplayName { get; private set; }

        public ElementChangeCollection<TElement> GetChanges()
        {
            return changes;
        }
    }
}
