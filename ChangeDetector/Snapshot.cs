using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal class Snapshot
    {
        public static readonly Snapshot Null = new Snapshot();

        private readonly Dictionary<PropertyInfo, object> lookup;

        public Snapshot()
        {
            this.lookup = new Dictionary<PropertyInfo, object>();
        }

        public void Add(PropertyInfo property, object value)
        {
            lookup.Add(property, value);
        }

        public SnapshotValue GetValue(PropertyInfo property)
        {
            if (this == Null)
            {
                return SnapshotValue.Null;
            }
            if (!lookup.ContainsKey(property))
            {
                return SnapshotValue.Missing;
            }
            return new SnapshotValue(lookup[property]);
        }
    }
}
