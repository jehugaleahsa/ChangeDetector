using System;
using System.Collections.Generic;
using System.Linq;

namespace ChangeDetector
{
    public interface ICollectionChangeDetector<TElement>
    {
        IElementChangeCollection<TElement> GetChanges(ICollection<TElement> original, ICollection<TElement> updated);

        IElementChangeCollection<TElement> GetChanges(ICollection<TElement> original, ICollection<TElement> updated, ElementState state);
    }

    public class CollectionChangeDetector<TElement> : ICollectionChangeDetector<TElement>
    {
        private readonly IEqualityComparer<TElement> comparer;

        public CollectionChangeDetector(IEqualityComparer<TElement> comparer = null)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TElement>.Default;
            }
            this.comparer = comparer;
        }

        public IElementChangeCollection<TElement> GetChanges(ICollection<TElement> original, ICollection<TElement> updated)
        {
            return GetChanges(original, updated, ElementState.Added | ElementState.Removed);
        }

        public IElementChangeCollection<TElement> GetChanges(ICollection<TElement> original, ICollection<TElement> updated, ElementState state)
        {
            HashSet<TElement> source = toHashSet(original);
            if (updated == null)
            {
                updated = new TElement[0];
            }
            HashSet<TElement> unmodified = new HashSet<TElement>(comparer);
            HashSet<TElement> added = new HashSet<TElement>(comparer);
            foreach (TElement element in updated)
            {
                if (source.Contains(element))
                {
                    unmodified.Add(element);
                }
                else
                {
                    added.Add(element);
                }
            }
            HashSet<TElement> removed = new HashSet<TElement>(comparer);
            foreach (TElement element in source)
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
            return new ElementChangeCollection<TElement>(changes, comparer);
        }

        private HashSet<TElement> toHashSet(ICollection<TElement> collection)
        {
            if (collection == null)
            {
                return new HashSet<TElement>(comparer);
            }
            HashSet<TElement> set = collection as HashSet<TElement>;
            if (set == null || set.Comparer != comparer)
            {
                return new HashSet<TElement>(collection, comparer);
            }
            return set;
        }

        private IEnumerable<IElementChange<TElement>> getElementChanges(HashSet<TElement> collection, ElementState filter, ElementState state)
        {
            if (!filter.HasFlag(state))
            {
                return Enumerable.Empty<ElementChange<TElement>>();
            }
            return collection.Select(e => new ElementChange<TElement>(e, state));
        }
    }
}
