using UnityEngine;

namespace Core.Gui
{
    public interface ICarouselFactory<TData>
    {
        ICarouselItem<TData> Create(RectTransform parent);
    }
}
