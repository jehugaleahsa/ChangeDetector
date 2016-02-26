using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChangeDetector
{
    public class ElementChangeCollection<TElement> : IEnumerable<ElementChange<TElement>>
    {
        private Dictionary<TElement, ElementChange<TElement>> lookup;

        internal ElementChangeCollection(IEnumerable<ElementChange<TElement>> changes, IEqualityComparer<TElement> comparer)
        {
            this.lookup = changes.ToDictionary(c => c.Item, comparer);
        }

        public ElementChange<TElement> GetChange(TElement element)
        {
            ElementChange<TElement> change;
            if (lookup.TryGetValue(element, out change))
            {
                return change;
            }
            else
            {
                return null;
            }
        }

        public int Count
        {
            get { return lookup.Count; }
        }

        public IEnumerator<ElementChange<TElement>> GetEnumerator()
        {
            return lookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
