using System;
using System.Diagnostics;
using System.Reflection;

namespace ChangeDetector
{
    [DebuggerDisplay("{FieldName,nq}: {OldValue,nq} -> {NewValue,nq}")]
    public class FieldChange
    {
        public PropertyInfo Property { get; internal set; }

        public string FieldName { get; internal set; }

        public string OldValue { get; internal set; }

        public string NewValue { get; internal set; }
    }
}
