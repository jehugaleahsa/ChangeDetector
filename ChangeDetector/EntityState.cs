using System;

namespace ChangeDetector
{
    [Flags]
    public enum EntityState
    {
        Detached = 0,
        Added = 1,
        Unmodified = 2,
        Modified = 4,
        Removed = 8
    }
}
