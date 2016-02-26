using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeDetector
{
    internal class Entity<TEntity>
        where TEntity : class
    {
        public TEntity Instance { get; set; }

        public EntityState State { get; set; }

        public Snapshot Snapshot { get; set; }

        public CollectionSnapshotLookup CollectionSnapshots { get; set; }
    }
}
