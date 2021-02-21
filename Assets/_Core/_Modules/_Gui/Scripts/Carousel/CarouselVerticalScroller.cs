using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Gui.Carousel
{
    public class CarouselVerticalScroller : CarouselScroller
    {
        [SerializeField]
        private Scrollbar _verticalScrollbar;
        public Scrollbar VerticalScrollbar
        {
            get
            {
                return _verticalScrollbar;
            }
            set
            {
                if (_verticalScrollbar)
                    _verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                _verticalScrollbar = value;
                if (_verticalScrollbar)
                    _verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
            }
        }

        protected override void ScrollContentToRect(RectTransform rect)
        {
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, (rect.pivot.y - 1) * rect.sizeDelta.y - rect.anchoredPosition.y);
        }

        protected override void Awake()
        {
            if (!Looped)
            { 
                _content.pivot = new Vector2(0.5f, 1f);
                _content.anchorMin = _content.anchorMax = new Vector2(0.5f, 1f);
                _content.anchoredPosition = Vector2.zero;
            }
            else
                base.Awake();
        }

        protected override void OnEnable()
        {
            if (_verticalScrollbar)
                _verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (_verticalScrollbar)
                _verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            base.OnDisable();
        }

        protected override void UpdateScrollbars(Vector2 offset)
        {
            if (_verticalScrollbar)
            {
                if (_contentBounds.size.y > 0)
                    _verticalScrollbar.size = Mathf.Clamp01((_viewBounds.size.y - Mathf.Abs(offset.y)) / _contentBounds.size.y);
                else
                    _verticalScrollbar.size = 1;

                _verticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        protected override Vector2 GetScrollDelta(PointerEventData data)
        {
            var delta = base.GetScrollDelta(data);
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                delta.y = delta.x;
            delta.x = 0;
            return delta;
        }

        protected override void CorrectAnchoredPosition(ref Vector2 position)
        {
            position.x = _content.anchoredPosition.x;
        }

        protected override bool TrySwap(Vector2 scrollRectMin, Vector2 scrollRectMax, out Vector2? swapedItemSize)
        {
            if (Content.rect.height > ViewRect.rect.size.y)
            {
                if (Content.anchoredPosition.y - _contentPadding.bottom + Content.rect.height*(1 - Content.pivot.y) < scrollRectMax.y)
                {
                    if (_swapBackwardHandler != null)
                    {
                        var result = _swapBackwardHandler();
                        if (result == null)
                        {
                            swapedItemSize = null;
                            return false;
                        }
                        swapedItemSize = new Vector2(0, result.Value.y);
                        return true;
                    }
                }
                else if (Content.anchoredPosition.y + _contentPadding.top - Content.rect.height*Content.pivot.y > scrollRectMin.y)
                {
                    if (_swapForwardHandler != null)
                    {
                        var result = _swapForwardHandler();
                        if (result == null)
                        {
                            swapedItemSize = null;
                            return false;
                        }
                        swapedItemSize = new Vector2(0, -result.Value.y);
                        return true;
                    }
                }
            }

            swapedItemSize = null;
            return false;
        }

        protected override Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (_movement == MovementType.Unrestricted)
                return offset;

            Vector2 min = _contentBounds.min;
            Vector2 max = _contentBounds.max;

            min.y += delta.y;
            max.y += delta.y;
            if (max.y < _viewBounds.max.y)
                offset.y = _viewBounds.max.y - max.y;
            else if (min.y > _viewBounds.min.y)
                offset.y = _viewBounds.min.y - min.y;
            

            return offset;
        }


        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }
    }
}
