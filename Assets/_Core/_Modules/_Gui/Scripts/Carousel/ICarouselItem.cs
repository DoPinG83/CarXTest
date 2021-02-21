using System;
using UnityEngine;

namespace Core.Gui
{
    public interface ICarouselItem<TData> : IDisposable
    {
        TData Data { get; set;}
        RectTransform Rect { get; }

        void Init(TData data);

        void Clear();
    }
}
