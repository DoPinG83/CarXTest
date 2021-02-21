using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Gui.Carousel
{
    internal class CarouselHorizontalScroller : CarouselScroller
    {
        [SerializeField]
        private Scrollbar _horizontalScrollbar;

        [SerializeField] private bool _alignToCenter;
        public Scrollbar HorizontalScrollbar
        {
            get
            {
                return _horizontalScrollbar;
            }
            set
            {
                if (_horizontalScrollbar)
                    _horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                _horizontalScrollbar = value;
                if (_horizontalScrollbar)
                    _horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            }
        }

        protected override void ScrollContentToRect(RectTransform rect)
        {
            _content.anchoredPosition = new Vector2(rect.pivot.x * rect.sizeDelta.x - rect.anchoredPosition.x, _content.anchoredPosition.y);
        }

        protected override void Awake()
        {
            if (!Looped)
            {
                _content.pivot = new Vector2(0f, 0.5f);
                if (!_alignToCenter)
                    _content.anchorMin = _content.anchorMax = new Vector2(0f, 0.5f);
                else if (_content.GetComponent<LayoutElement>() != null)
                    StartCoroutine(ReAlignToCenter());

                _content.anchoredPosition = Vector2.zero;
            }
            else
                base.Awake();
        }

        private IEnumerator ReAlignToCenter()
        {
            yield return new WaitForEndOfFrame();
            _content.GetComponent<LayoutElement>().minWidth = gameObject.RectTransform().rect.width;
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.RectTransform());
        }

        protected override void OnEnable()
        {
            if (_horizontalScrollbar)
                _horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (_horizontalScrollbar)
                _horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);

            base.OnDisable();
        }

        protected override void UpdateScrollbars(Vector2 offset)
        {
            if (_horizontalScrollbar)
            {
                if (_contentBounds.size.x > 0)
                    _horizontalScrollbar.size = Mathf.Clamp01((_viewBounds.size.x - Mathf.Abs(offset.x)) / _contentBounds.size.x);
                else
                    _horizontalScrollbar.size = 1;

                _horizontalScrollbar.value = horizontalNormalizedPosition;
            }
        }

        protected override Vector2 GetScrollDelta(PointerEventData data)
        {
            var delta = base.GetScrollDelta(data);
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                delta.x = delta.y;
            delta.y = 0;
            return delta;
        }

        protected override void CorrectAnchoredPosition(ref Vector2 position)
        {
            position.y = _content.anchoredPosition.y;
        }

        protected override bool TrySwap(Vector2 scrollRectMin, Vector2 scrollRectMax, out Vector2? swapedItemSize)
        {
            if (Content.rect.width > ViewRect.rect.size.x)
            {
                var lastItemPosMax = Content.anchoredPosition.x - _contentPadding.right + Content.rect.width * (1 - Content.pivot.x);

                if (lastItemPosMax < scrollRectMax.x)
                {
                    if (_swapForwardHandler != null)
                    {
                        var result = _swapForwardHandler();
                        if (result == null)
                        {
                            swapedItemSize = null;
                            return false;
                        }
                        swapedItemSize = new Vector2(result.Value.x, 0);
                        return true;
                    }
                }
                else
                {
                    var firstItemPosMin = Content.anchoredPosition.x + _contentPadding.left - Content.rect.width * Content.pivot.x;
                    if (firstItemPosMin > scrollRectMin.x)
                    {
                        if (_swapBackwardHandler != null)
                        {
                            var result = _swapBackwardHandler();
                            if (result == null)
                            {
                                swapedItemSize = null;
                                return false;
                            }
                            swapedItemSize = new Vector2(-result.Value.x, 0);
                            return true;
                        }
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

            min.x += delta.x;
            max.x += delta.x;
            if (min.x > _viewBounds.min.x)
                offset.x = _viewBounds.min.x - min.x;
            else if (max.x < _viewBounds.max.x)
                offset.x = _viewBounds.max.x - max.x;

            return offset;
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
    }
}
