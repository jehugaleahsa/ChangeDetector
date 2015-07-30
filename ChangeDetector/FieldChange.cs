using System;
using System.Diagnostics;

namespace ChangeDetector
{
    [DebuggerDisplay("{FieldName,nq}: {OldValue,nq} -> {NewValue,nq}")]
    public class FieldChange
    {
        public string FieldName { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }
    }
}
