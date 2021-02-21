using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Gui.Carousel
{
    public class CarouselChatScroller : CarouselVerticalScroller
    {


        protected override void Awake()
        {
            if (!Looped){
                _content.pivot = new Vector2(0.5f, 0f);
                _content.anchorMin = _content.anchorMax = new Vector2(0.5f, 0f);
                _content.anchoredPosition = Vector2.zero;
            } else
                base.Awake();
        }

    }
}
