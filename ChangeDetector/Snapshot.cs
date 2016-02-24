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

        public bool HasValue(PropertyInfo property)
        {
            return IsNull() || lookup.ContainsKey(property);
        }

        public TValue GetValue<TValue>(PropertyInfo property)
        {
            if (lookup.ContainsKey(property))
            {
                return (TValue)lookup[property];
            }
            else
            {
                return default(TValue);
            }
        }

        public bool IsNull()
        {
            return Object.ReferenceEquals(this, Null);
        }
    }
}
