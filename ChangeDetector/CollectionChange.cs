using System;
using System.Reflection;

namespace ChangeDetector
{
    public interface ICollectionChange<TElement>
    {
        PropertyInfo Property { get; }

        string DisplayName { get; }

        IElementChangeCollection<TElement> GetChanges();
    }

    internal class CollectionChange<TElement> : ICollectionChange<TElement>
    {
        private readonly IElementChangeCollection<TElement> changes;

        public CollectionChange(
            PropertyInfo property, 
            string displayName, 
            IElementChangeCollection<TElement> changes)
        {
            this.Property = property;
            this.DisplayName = displayName;
            this.changes = changes;
        }

        public PropertyInfo Property { get; private set; }

        public string DisplayName { get; private set; }

        public IElementChangeCollection<TElement> GetChanges()
        {
            return changes;
        }
    }
}
