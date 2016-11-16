using System;

namespace ChangeDetector
{
    // Users can compare instances of different classes in the same hierarchy.
    // For instance, a user can compare an Animal to a Dog, or a Cat to a Dog.
    // We also need to support comparing an object to null, where we treat every property as null.
    // It is important to mention that Null and null do not mean the same thing.
    // Null means that the original entity was null, where null means the entity's property value is null.
    // Furthermore, Missing means that the property is not on the entity whatsoever.
    internal class SnapshotValue
    {
        public static readonly SnapshotValue Null = new SnapshotValue(null);
        public static readonly SnapshotValue Missing = new SnapshotValue(null);

        private readonly Snapshot snapshot;
        private readonly bool hasValue;
        private readonly object value;

        private SnapshotValue(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public SnapshotValue(Snapshot snapshot, object value)
        {
            this.snapshot = snapshot;
            this.hasValue = true;
            this.value = value;
        }

        public Snapshot Snapshot
        {
            get { return snapshot; }
        }

        public bool IsNull()
        {
            return !hasValue || value == null;
        }

        public bool HasValue()
        {
            return hasValue;
        }

        public TValue GetValue<TValue>()
        {
            return hasValue ? (TValue)value : default(TValue);
        }
    }
}
