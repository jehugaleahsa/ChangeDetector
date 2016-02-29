using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChangeDetector
{
    public interface IElementChangeCollection<TElement> : IEnumerable<IElementChange<TElement>>
    {
        int Count { get; }

        IElementChange<TElement> GetChange(TElement element);
    }

    internal class ElementChangeCollection<TElement> : IElementChangeCollection<TElement>
    {
        private Dictionary<TElement, IElementChange<TElement>> lookup;

        internal ElementChangeCollection(IEnumerable<IElementChange<TElement>> changes, IEqualityComparer<TElement> comparer)
        {
            this.lookup = changes.ToDictionary(c => c.Item, comparer);
        }

        public int Count
        {
            get { return lookup.Count; }
        }

        public IElementChange<TElement> GetChange(TElement element)
        {
            IElementChange<TElement> change;
            if (lookup.TryGetValue(element, out change))
            {
                return change;
            }
            else
            {
                return null;
            }
        }

        public IEnumerator<IElementChange<TElement>> GetEnumerator()
        {
            return lookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
