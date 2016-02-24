using System;
using System.Diagnostics;
using System.Reflection;

namespace ChangeDetector
{
    public interface IFieldChange
    {
        PropertyInfo Property { get; }

        string FieldName { get; }

        object OriginalValue { get; }

        object UpdatedValue { get; }

        string FormatOriginalValue();

        string FormatUpdatedValue();
    }

    [DebuggerDisplay("{FieldName,nq}: {OriginalValue,nq} -> {UpdatedValue,nq}")]
    internal class FieldChange<TEntity, TProp> : IFieldChange
        where TEntity : class
    {
        private readonly PropertyConfiguration<TEntity, TProp> configuration;
        private readonly TProp original;
        private readonly TProp updated;

        public FieldChange(PropertyConfiguration<TEntity, TProp> configuration, TProp original, TProp updated)
        {
            this.configuration = configuration;
            this.original = original;
            this.updated = updated;
        }

        public PropertyInfo Property
        {
            get { return configuration.Property; }
        }

        public string FieldName
        {
            get { return configuration.DisplayName; }
        }

        object IFieldChange.OriginalValue
        {
            get { return original; }
        }

        object IFieldChange.UpdatedValue
        {
            get { return updated; }
        }

        public string FormatOriginalValue()
        {
            return formatValue(original);
        }

        public string FormatUpdatedValue()
        {
            return formatValue(updated);
        }

        private string formatValue(TProp value)
        {
            return configuration.Formatter(value);
        }
    }
}
