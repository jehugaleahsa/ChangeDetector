using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal interface ICollectionConfiguration
    {
        PropertyInfo Property { get; }

        Type ElementType { get; }

        string DiplayName { get; }

        bool IsValueSource(object entity);

        object GetCollectionCopy(object entity);
    }

    internal class CollectionConfiguration<TElement> : ICollectionConfiguration
    {
        public CollectionConfiguration(PropertyInfo property, string displayName, IEqualityComparer<TElement> comparer)
        {
            this.Property = property;
            this.DiplayName = displayName;
            this.Comparer = comparer;
        }

        public PropertyInfo Property { get; private set; }

        public string DiplayName { get; private set; }

        public Type ElementType 
        { 
            get { return typeof(TElement); } 
        }

        public IEqualityComparer<TElement> Comparer { get; private set; }

        public bool IsValueSource(object entity)
        {
            return entity != null && Property.DeclaringType.IsAssignableFrom(entity.GetType());
        }

        public ICollection<TElement> GetCollection(object entity)
        {
            return (ICollection<TElement>)Property.GetValue(entity);
        }

        public object GetCollectionCopy(object entity)
        {
            var original = (IEnumerable<TElement>)Property.GetValue(entity);
            if (original == null)
            {
                return null;
            }
            var copy = new HashSet<TElement>(original, Comparer);
            return copy;
        }
    }
}
