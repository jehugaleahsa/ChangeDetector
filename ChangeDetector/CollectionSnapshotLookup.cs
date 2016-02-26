using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal class CollectionSnapshotLookup
    {
        private readonly Dictionary<PropertyInfo, object> snapshots;

        public CollectionSnapshotLookup()
        {
            this.snapshots = new Dictionary<PropertyInfo, object>();
        }

        public void Add(PropertyInfo property, object collection)
        {
            this.snapshots[property] = collection;
        }

        public ICollection<TElement> GetSnapshot<TElement>(PropertyInfo property)
        {
            object collection;
            if (snapshots.TryGetValue(property, out collection))
            {
                return (ICollection<TElement>)collection;
            }
            else
            {
                return null;
            }
        }
    }
}
