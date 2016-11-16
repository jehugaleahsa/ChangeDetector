using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal class Snapshot
    {
        public static readonly Snapshot Null = new Snapshot(null);

        private readonly object entity;
        private readonly Dictionary<PropertyInfo, object> lookup;

        public Snapshot(object entity)
        {
            this.entity = entity;
            this.lookup = new Dictionary<PropertyInfo, object>();
        }

        internal object Entity
        {
            get { return entity; }
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
            return new SnapshotValue(this, lookup[property]);
        }
    }
}
