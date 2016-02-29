using System;

namespace ChangeDetector
{
    public interface IElementChange<TElement>
    {
        TElement Item { get; }

        ElementState State { get; }
    }

    internal class ElementChange<TElement> : IElementChange<TElement>
    {
        public ElementChange(TElement element, ElementState state)
        {
            this.Item = element;
            this.State = state;
        }

        public TElement Item { get; private set;  }

        public ElementState State { get; private set; }
    }
}
