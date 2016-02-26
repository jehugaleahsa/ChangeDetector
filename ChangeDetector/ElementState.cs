using System;

namespace ChangeDetector
{
    [Flags]
    public enum ElementState
    {
        Detached = 0,
        Added = 1,
        Unmodified = 2,
        Removed = 8
    }
}
