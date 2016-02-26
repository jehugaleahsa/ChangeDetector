using System;

namespace ChangeDetector
{
    public class ElementChange<TElement>
    {
        internal ElementChange(TElement element, ElementState state)
        {
            this.Item = element;
            this.State = state;
        }

        public TElement Item { get; private set;  }

        public ElementState State { get; private set; }
    }
}
