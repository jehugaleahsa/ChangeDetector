using System;
using System.Collections.Generic;
using System.Linq;

namespace ChangeDetector
{
    public class CollectionChangeDetector<TElement>
    {
        private readonly HashSet<TElement> collection;

        public CollectionChangeDetector(IEnumerable<TElement> collection, IEqualityComparer<TElement> comparer = null)
        {
            if (collection == null)
            {
                collection = new TElement[0];
            }
            if (comparer == null)
            {
                comparer = EqualityComparer<TElement>.Default;
            }
            this.collection = new HashSet<TElement>(collection, comparer);
        }

        public ElementChangeCollection<TElement> GetChanges(IEnumerable<TElement> updated)
        {
            return GetChanges(updated, ElementState.Added | ElementState.Removed);
        }

        public ElementChangeCollection<TElement> GetChanges(IEnumerable<TElement> updated, ElementState state)
        {
            if (updated == null)
            {
                updated = new TElement[0];
            }
            HashSet<TElement> unmodified = new HashSet<TElement>(collection.Comparer);
            HashSet<TElement> added = new HashSet<TElement>(collection.Comparer);
            foreach (TElement element in updated)
            {
                if (collection.Contains(element))
                {
                    unmodified.Add(element);
                }
                else
                {
                    added.Add(element);
                }
            }
            HashSet<TElement> removed = new HashSet<TElement>(collection.Comparer);
            foreach (TElement element in collection)
            {
                if (!unmodified.Contains(element))
                {
                    removed.Add(element);
                }
            }
            var unmodifiedChanges = getElementChanges(unmodified, state, ElementState.Unmodified);
            var addedChanges = getElementChanges(added, state, ElementState.Added);
            var removedChanges = getElementChanges(removed, state, ElementState.Removed);
            var changes = unmodifiedChanges.Concat(addedChanges).Concat(removedChanges);
            return new ElementChangeCollection<TElement>(changes, collection.Comparer);
        }

        private IEnumerable<ElementChange<TElement>> getElementChanges(HashSet<TElement> collection, ElementState filter, ElementState state)
        {
            if (!filter.HasFlag(state))
            {
                return Enumerable.Empty<ElementChange<TElement>>();
            }
            return collection.Select(e => new ElementChange<TElement>(e, state));
        }
    }
}
