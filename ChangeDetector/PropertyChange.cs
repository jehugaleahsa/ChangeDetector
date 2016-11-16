using System.Diagnostics;
using System.Reflection;

namespace ChangeDetector
{
    public interface IPropertyChange
    {
        PropertyInfo Property { get; }

        string DisplayName { get; }

        object OriginalValue { get; }

        object UpdatedValue { get; }

        string FormatOriginalValue();

        string FormatUpdatedValue();
    }

    [DebuggerDisplay("{DisplayName,nq}: {FormatOriginalValue(),nq} -> {FormatUpdatedValue(),nq}")]
    internal class PropertyChange<TProp> : IPropertyChange
    {
        private readonly PropertyConfiguration<TProp> configuration;
        private readonly SnapshotValue original;
        private readonly SnapshotValue updated;

        public PropertyChange(PropertyConfiguration<TProp> configuration, SnapshotValue original, SnapshotValue updated)
        {
            this.configuration = configuration;
            this.original = original;
            this.updated = updated;
        }

        public PropertyInfo Property
        {
            get { return configuration.Property; }
        }

        public string DisplayName
        {
            get { return configuration.DisplayName(updated?.Snapshot?.Entity); }
        }

        public object OriginalValue
        {
            get { return original.GetValue<object>(); }
        }

        public object UpdatedValue
        {
            get { return updated.GetValue<object>(); }
        }

        public string FormatOriginalValue()
        {
            return formatValue(original);
        }

        public string FormatUpdatedValue()
        {
            return formatValue(updated);
        }

        private string formatValue(SnapshotValue value)
        {
            object entity = updated?.Snapshot?.Entity;
            return value.HasValue() ? configuration.Formatter(entity, value.GetValue<TProp>()) : null;
        }
    }
}
